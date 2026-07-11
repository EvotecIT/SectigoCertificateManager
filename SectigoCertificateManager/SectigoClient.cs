namespace SectigoCertificateManager;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Provides a basic HTTP client wrapper for communicating with the Sectigo API.
/// </summary>
public sealed class SectigoClient : ISectigoClient, IDisposable {
    private readonly HttpClient _client;
    private readonly bool _ownsHttpClient;
    private readonly Func<CancellationToken, Task<TokenInfo>>? _refreshToken;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly SemaphoreSlim? _throttle;
    private readonly int _retryCount;
    private readonly TimeSpan _retryInitialDelay;
    private readonly TimeSpan _tokenRefreshThreshold;
    internal Func<TimeSpan, CancellationToken, Task>? DelayAsync { get; set; }
    private string? _token;
    private DateTimeOffset? _tokenExpiresAt;
    private readonly string? _tokenCachePath;
    private bool _disposed;

    private void ThrowIfDisposed() {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(SectigoClient));
        }
    }

    /// <summary>
    /// Gets the underlying <see cref="HttpClient"/> instance used for requests.
    /// </summary>
    public HttpClient HttpClient {
        get {
            ThrowIfDisposed();
            return _client;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SectigoClient"/> class.
    /// </summary>
    /// <param name="config">API configuration settings.</param>
    /// <param name="httpClient">Optional pre-configured HTTP client.</param>
    public SectigoClient(ApiConfig config, HttpClient? httpClient = null) {
        if (httpClient is null) {
            var handler = new HttpClientHandler();
            if (config.ClientCertificate is not null) {
                handler.ClientCertificates.Add(config.ClientCertificate);
            }

            config.ConfigureHandler?.Invoke(handler);
            httpClient = new HttpClient(handler);
            _ownsHttpClient = true;
        }

        _client = httpClient;
        string baseUrl = config.BaseUrl;
        if (!baseUrl.EndsWith("/", StringComparison.Ordinal)) {
            baseUrl += "/";
        }
        _client.BaseAddress = new Uri(baseUrl);
        _refreshToken = config.RefreshToken;
        _token = config.Token;
        _tokenExpiresAt = config.TokenExpiresAt;
        _retryCount = config.RetryCount;
        _retryInitialDelay = config.RetryInitialDelay;
        _tokenRefreshThreshold = config.TokenRefreshThreshold;
        _tokenCachePath = config.TokenCachePath;
        if (config.ConcurrencyLimit.HasValue) {
            _throttle = new SemaphoreSlim(config.ConcurrencyLimit.Value, config.ConcurrencyLimit.Value);
        }
        ConfigureHeaders(config);
    }

    /// <summary>
    /// Sends a GET request to the specified endpoint.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default) {
        ThrowIfDisposed();
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        if (_throttle is not null) {
            await _throttle.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        try {
            return await SendCheckedAsync(HttpMethod.Get, requestUri, content: null, cancellationToken).ConfigureAwait(false);
        } finally {
            _throttle?.Release();
        }
    }

    /// <summary>
    /// Sends a POST request with the provided content.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="content">HTTP content to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default) {
        ThrowIfDisposed();
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        if (_throttle is not null) {
            await _throttle.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        try {
            return await SendCheckedAsync(HttpMethod.Post, requestUri, content, cancellationToken).ConfigureAwait(false);
        } finally {
            _throttle?.Release();
        }
    }

    /// <summary>
    /// Sends a PUT request with the provided content.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="content">HTTP content to send.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default) {
        ThrowIfDisposed();
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        if (_throttle is not null) {
            await _throttle.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        try {
            return await SendCheckedAsync(HttpMethod.Put, requestUri, content, cancellationToken).ConfigureAwait(false);
        } finally {
            _throttle?.Release();
        }
    }

    /// <summary>
    /// Sends a DELETE request to the specified endpoint.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default) {
        ThrowIfDisposed();
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        if (_throttle is not null) {
            await _throttle.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
        try {
            return await SendCheckedAsync(HttpMethod.Delete, requestUri, content: null, cancellationToken).ConfigureAwait(false);
        } finally {
            _throttle?.Release();
        }
    }

    private async Task<HttpResponseMessage> SendCheckedAsync(
        HttpMethod method,
        string requestUri,
        HttpContent? content,
        CancellationToken cancellationToken) {
        if (!RequestSnapshot.CanReplaySafely(method, content)) {
            return await SendCheckedOnceAsync(method, requestUri, content, cancellationToken).ConfigureAwait(false);
        }

        var snapshot = await CreateSnapshotAsync(method, requestUri, content).ConfigureAwait(false);
        return await SendCheckedAsync(snapshot, cancellationToken).ConfigureAwait(false);
    }

    private async Task<HttpResponseMessage> SendCheckedAsync(
        RequestSnapshot snapshot,
        CancellationToken cancellationToken) {
        var response = await SendWithRetryAsync(snapshot, cancellationToken).ConfigureAwait(false);
        try {
            await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return response;
        } catch {
            response.Dispose();
            throw;
        }
    }

    private async Task<HttpResponseMessage> SendCheckedOnceAsync(
        HttpMethod method,
        string requestUri,
        HttpContent? content,
        CancellationToken cancellationToken) {
        using var request = new HttpRequestMessage(method, requestUri) {
            Content = content is null ? null : new NonDisposingHttpContent(content)
        };
        HttpResponseMessage response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);

        try {
            await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return response;
        } catch {
            response.Dispose();
            throw;
        }
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        RequestSnapshot snapshot,
        CancellationToken cancellationToken) {
        var attempt = 0;
        var delay = _retryInitialDelay;
        while (true) {
            HttpResponseMessage response;
            try {
                using var request = snapshot.CreateRequest();
                response = await _client.SendAsync(request, cancellationToken).ConfigureAwait(false);
            } catch (HttpRequestException) when (attempt < _retryCount - 1) {
                await DelayBeforeRetryAsync(delay, cancellationToken).ConfigureAwait(false);
                attempt++;
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                continue;
            } catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested && attempt < _retryCount - 1) {
                await DelayBeforeRetryAsync(delay, cancellationToken).ConfigureAwait(false);
                attempt++;
                delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
                continue;
            }

            var status = (int)response.StatusCode;
            var retryable = status == 429 || (status >= 500 && status < 600 && response.StatusCode != HttpStatusCode.NotImplemented);
            if (!retryable || attempt >= _retryCount - 1) {
                return response;
            }

            var wait = delay;
            if (status == 429 && response.Headers.TryGetValues("Retry-After", out var values)) {
                var value = values.FirstOrDefault();
                if (int.TryParse(value, out var seconds)) {
                    wait = TimeSpan.FromSeconds(seconds);
                } else if (DateTimeOffset.TryParse(value, out var date)) {
                    wait = date - DateTimeOffset.UtcNow;
                    if (wait < TimeSpan.Zero) {
                        wait = TimeSpan.Zero;
                    }
                }
            }

            response.Dispose();

            await DelayBeforeRetryAsync(wait, cancellationToken).ConfigureAwait(false);

            attempt++;
            delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
        }
    }

    private Task DelayBeforeRetryAsync(TimeSpan delay, CancellationToken cancellationToken) =>
        DelayAsync is null ? Task.Delay(delay, cancellationToken) : DelayAsync(delay, cancellationToken);

    private static async Task<RequestSnapshot> CreateSnapshotAsync(
        HttpMethod method,
        string requestUri,
        HttpContent? content) {
        using var request = new HttpRequestMessage(method, requestUri);
        request.Content = content;
        var snapshot = await RequestSnapshot.CreateAsync(request).ConfigureAwait(false);
        request.Content = null;
        return snapshot;
    }

    private void ConfigureHeaders(ApiConfig cfg) {
        if (!_client.DefaultRequestHeaders.Accept.Any(static value =>
            string.Equals(value.MediaType, "application/json", StringComparison.OrdinalIgnoreCase))) {
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }
        _client.DefaultRequestHeaders.Remove("customerUri");
        _client.DefaultRequestHeaders.Remove("login");
        _client.DefaultRequestHeaders.Remove("password");
        _client.DefaultRequestHeaders.Add("customerUri", cfg.CustomerUri);

        if (cfg.Token is not null && cfg.Token.Length > 0) {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cfg.Token);
        } else {
            _client.DefaultRequestHeaders.Authorization = null;
            _client.DefaultRequestHeaders.Add("login", cfg.Username);
            _client.DefaultRequestHeaders.Add("password", cfg.Password);
        }
    }

    private async Task EnsureValidTokenAsync(CancellationToken cancellationToken) {
        ThrowIfDisposed();
        if (_refreshToken is null) {
            return;
        }

        if (_token is not null && _tokenExpiresAt is not null && _tokenExpiresAt - DateTimeOffset.UtcNow > _tokenRefreshThreshold) {
            return;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            ThrowIfDisposed();
            if (_token is not null && _tokenExpiresAt is not null && _tokenExpiresAt - DateTimeOffset.UtcNow > _tokenRefreshThreshold) {
                return;
            }

            var info = await _refreshToken(cancellationToken).ConfigureAwait(false);
            _token = info.Token;
            _tokenExpiresAt = info.ExpiresAt;
            ApiConfigLoader.WriteToken(info, _tokenCachePath);
            _client.DefaultRequestHeaders.Remove("login");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", info.Token);
        } finally {
            if (!_disposed) {
                _refreshLock.Release();
            }
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposed) {
            return;
        }

        if (_ownsHttpClient) {
            _client.Dispose();
        }
        _refreshLock.Dispose();
        _throttle?.Dispose();
        _disposed = true;
    }
}
