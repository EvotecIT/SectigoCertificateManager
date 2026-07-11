namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Shared infrastructure for Admin Operations API clients (HTTP client + token management).
/// </summary>
public abstract class AdminApiClientBase : IDisposable {
    /// <summary>Resolved Admin API configuration (base URL and OAuth2 client credentials).</summary>
    protected readonly AdminApiConfig _config;

    /// <summary>HTTP client used for all Admin Operations API requests.</summary>
    protected readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly AdminTokenManager _tokenManager;
    internal Func<TimeSpan, CancellationToken, Task>? DelayAsync { get; set; }

    /// <summary>
    /// Initializes a new Admin Operations API client base with the specified configuration and HTTP client.
    /// </summary>
    /// <param name="config">Admin API configuration (base URL and OAuth2 client credentials).</param>
    /// <param name="httpClient">Optional HTTP client instance; when omitted a new one is created and disposed with the client.</param>
    protected AdminApiClientBase(AdminApiConfig config, HttpClient? httpClient = null) {
        _config = Guard.AgainstNull(config, nameof(config));

        if (httpClient is null) {
            _httpClient = new HttpClient();
            _ownsHttpClient = true;
        } else {
            _httpClient = httpClient;
            _ownsHttpClient = false;
        }

        if (!_config.BaseUrl.EndsWith("/", StringComparison.Ordinal)) {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl + "/");
        } else {
            _httpClient.BaseAddress = new Uri(_config.BaseUrl);
        }

        _tokenManager = new AdminTokenManager(_httpClient, _config, DelayBeforeRetryAsync);
    }

    /// <summary>
    /// Retrieves an OAuth2 access token, using cached value when possible.
    /// </summary>
    protected Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) =>
        _tokenManager.GetTokenAsync(cancellationToken);

    /// <summary>
    /// Adds a Bearer token Authorization header to the specified request.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    /// <param name="token">The OAuth2 access token.</param>
    protected static void SetBearer(HttpRequestMessage request, string token) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Sends an Admin API request with bounded retries for rate limiting, server failures,
    /// transient transport errors, and timeouts that were not requested by the caller.
    /// </summary>
    protected async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken) =>
        await SendAsync(request, HttpCompletionOption.ResponseContentRead, cancellationToken).ConfigureAwait(false);

    /// <summary>Sends a retryable Admin API request with an explicit response buffering policy.</summary>
    protected async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        HttpCompletionOption completionOption,
        CancellationToken cancellationToken) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        if (!RequestSnapshot.CanReplaySafely(request.Method, request.Content)) {
            return await _httpClient.SendAsync(request, completionOption, cancellationToken).ConfigureAwait(false);
        }

        var snapshot = await RequestSnapshot.CreateAsync(request).ConfigureAwait(false);
        var attempts = Math.Max(1, _config.RetryCount);
        var delay = _config.RetryInitialDelay < TimeSpan.Zero ? TimeSpan.Zero : _config.RetryInitialDelay;

        for (var attempt = 0; ; attempt++) {
            cancellationToken.ThrowIfCancellationRequested();
            try {
                using var currentRequest = snapshot.CreateRequest();
                var response = await _httpClient.SendAsync(currentRequest, completionOption, cancellationToken).ConfigureAwait(false);
                if (!IsRetryable(response.StatusCode) || attempt >= attempts - 1) {
                    return response;
                }

                var wait = GetRetryDelay(response, delay);
                response.Dispose();
                await DelayBeforeRetryAsync(wait, cancellationToken).ConfigureAwait(false);
            } catch (HttpRequestException) when (attempt < attempts - 1) {
                await DelayBeforeRetryAsync(delay, cancellationToken).ConfigureAwait(false);
            } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < attempts - 1) {
                await DelayBeforeRetryAsync(delay, cancellationToken).ConfigureAwait(false);
            }

            delay = TimeSpan.FromTicks(Math.Min(TimeSpan.FromMinutes(1).Ticks, Math.Max(1, delay.Ticks * 2)));
        }
    }

    private Task DelayBeforeRetryAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        DelayAsync is null ? Task.Delay(delay, cancellationToken) : DelayAsync(delay, cancellationToken);

    private static bool IsRetryable(HttpStatusCode statusCode) {
        var status = (int)statusCode;
        return status == 429 || status >= 500 && status < 600 && statusCode != HttpStatusCode.NotImplemented;
    }

    internal static TimeSpan GetRetryDelay(HttpResponseMessage response, TimeSpan fallback) {
        RetryConditionHeaderValue? retryAfter = response.Headers.RetryAfter;
        if (retryAfter?.Delta is TimeSpan delta) {
            return delta < TimeSpan.Zero ? TimeSpan.Zero : delta;
        }

        if (retryAfter?.Date is DateTimeOffset date) {
            var delay = date - DateTimeOffset.UtcNow;
            return delay < TimeSpan.Zero ? TimeSpan.Zero : delay;
        }

        return fallback;
    }

    /// <inheritdoc />
    public void Dispose() {
        _tokenManager.Dispose();
        if (_ownsHttpClient) {
            _httpClient.Dispose();
        }
    }
}

internal sealed class RequestSnapshot {
    private readonly HttpMethod _method;
    private readonly Uri? _requestUri;
    private readonly Version _version;
    private readonly IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> _headers;
    private readonly byte[]? _content;
    private readonly IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> _contentHeaders;

    private RequestSnapshot(
        HttpMethod method,
        Uri? requestUri,
        Version version,
        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> headers,
        byte[]? content,
        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> contentHeaders) {
        _method = method;
        _requestUri = requestUri;
        _version = version;
        _headers = headers;
        _content = content;
        _contentHeaders = contentHeaders;
    }

    public static async Task<RequestSnapshot> CreateAsync(HttpRequestMessage request) {
        byte[]? content = null;
        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> contentHeaders = Array.Empty<KeyValuePair<string, IEnumerable<string>>>();
        if (request.Content is not null) {
            content = await request.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
            contentHeaders = CopyHeaders(request.Content.Headers);
        }

        return new RequestSnapshot(
            request.Method,
            request.RequestUri,
            request.Version,
            CopyHeaders(request.Headers),
            content,
            contentHeaders);
    }

    /// <summary>
    /// Returns whether a request can be buffered and replayed without duplicating a mutation
    /// or consuming a streaming body before the network send.
    /// </summary>
    public static bool CanReplaySafely(HttpMethod method, HttpContent? content) {
        bool idempotentMethod = method == HttpMethod.Get
            || method == HttpMethod.Head
            || method == HttpMethod.Options
            || method == HttpMethod.Put
            || method == HttpMethod.Delete;
        if (!idempotentMethod) {
            return false;
        }

        return content is null
            || content is ByteArrayContent
            || content is StringContent
            || content is FormUrlEncodedContent
            || content is JsonContent;
    }

    public HttpRequestMessage CreateRequest() {
        var request = new HttpRequestMessage(_method, _requestUri) { Version = _version };
        CopyHeadersTo(_headers, request.Headers);
        if (_content is not null) {
            request.Content = new ByteArrayContent(_content);
            CopyHeadersTo(_contentHeaders, request.Content.Headers);
        }

        return request;
    }

    private static IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> CopyHeaders(HttpHeaders headers) =>
        headers.Select(static header =>
            new KeyValuePair<string, IEnumerable<string>>(header.Key, header.Value.ToArray())).ToArray();

    private static void CopyHeadersTo(
        IReadOnlyList<KeyValuePair<string, IEnumerable<string>>> source,
        HttpHeaders destination) {
        foreach (var header in source) {
            destination.TryAddWithoutValidation(header.Key, header.Value);
        }
    }
}

/// <summary>
/// Manages OAuth2 token retrieval and caching for Admin Operations API clients.
/// </summary>
internal sealed class AdminTokenManager : IDisposable {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);
    private const int DefaultTokenLifetimeSeconds = 300;
    private static readonly TimeSpan TokenExpirySkew = TimeSpan.FromMinutes(1);

    private readonly HttpClient _httpClient;
    private readonly AdminApiConfig _config;
    private readonly Func<TimeSpan, CancellationToken, Task> _delayAsync;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string _cachedToken = string.Empty;
    private DateTimeOffset _tokenExpiresAt;

    public AdminTokenManager(
        HttpClient httpClient,
        AdminApiConfig config,
        Func<TimeSpan, CancellationToken, Task> delayAsync) {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _delayAsync = delayAsync ?? throw new ArgumentNullException(nameof(delayAsync));
    }

    public async Task<string> GetTokenAsync(CancellationToken cancellationToken) {
        if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiresAt) {
            return _cachedToken;
        }

        await _lock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTimeOffset.UtcNow < _tokenExpiresAt) {
                return _cachedToken;
            }

            using var response = await SendTokenRequestAsync(cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var model = await response.Content
                .ReadFromJsonAsyncSafe<TokenResponse>(s_json, cancellationToken)
                .ConfigureAwait(false);
            if (model is null || string.IsNullOrWhiteSpace(model.AccessToken)) {
                throw new InvalidOperationException("Access token was not present in the Admin API token response.");
            }

            _cachedToken = model.AccessToken;
            var lifetimeSeconds = model.ExpiresIn > 0 ? model.ExpiresIn : DefaultTokenLifetimeSeconds;
            var expiry = DateTimeOffset.UtcNow.AddSeconds(lifetimeSeconds);
            // Refresh slightly before actual expiry to avoid edge conditions.
            _tokenExpiresAt = expiry - TokenExpirySkew;

            return _cachedToken;
        } finally {
            _lock.Release();
        }
    }

    private async Task<HttpResponseMessage> SendTokenRequestAsync(CancellationToken cancellationToken) {
        var attempts = Math.Max(1, _config.RetryCount);
        var delay = _config.RetryInitialDelay < TimeSpan.Zero ? TimeSpan.Zero : _config.RetryInitialDelay;
        for (var attempt = 0; ; attempt++) {
            var wait = delay;
            using var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret
            });

            try {
                var response = await _httpClient.PostAsync(_config.TokenUrl, content, cancellationToken).ConfigureAwait(false);
                var status = (int)response.StatusCode;
                if ((status != 429 && (status < 500 || status >= 600 || response.StatusCode == HttpStatusCode.NotImplemented)) || attempt >= attempts - 1) {
                    return response;
                }

                wait = AdminApiClientBase.GetRetryDelay(response, delay);
                response.Dispose();
            } catch (HttpRequestException ex) when (attempt < attempts - 1) {
                Trace.TraceWarning("Admin token request attempt {0} of {1} failed: {2}", attempt + 1, attempts, ex.Message);
            } catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested && attempt < attempts - 1) {
                Trace.TraceWarning("Admin token request attempt {0} of {1} timed out: {2}", attempt + 1, attempts, ex.Message);
            }

            await _delayAsync(wait, cancellationToken).ConfigureAwait(false);
            delay = TimeSpan.FromTicks(Math.Min(TimeSpan.FromMinutes(1).Ticks, Math.Max(1, delay.Ticks * 2)));
        }
    }

    public void Dispose() {
        _lock.Dispose();
    }

    private sealed class TokenResponse {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
