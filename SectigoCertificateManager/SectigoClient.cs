namespace SectigoCertificateManager;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides a basic HTTP client wrapper for communicating with the Sectigo API.
/// </summary>
public sealed class SectigoClient : ISectigoClient, IDisposable {
    private readonly HttpClient _client;
    private readonly Func<CancellationToken, Task<TokenInfo>>? _refreshToken;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);
    private readonly SemaphoreSlim? _throttle;
    private readonly int _retryCount;
    private readonly TimeSpan _retryInitialDelay;
    private readonly TimeSpan _tokenRefreshThreshold;
    internal Func<TimeSpan, CancellationToken, Task>? DelayAsync { get; set; }
    private string? _token;
    private DateTimeOffset? _tokenExpiresAt;
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
            var response = await SendWithRetryAsync(ct => _client.GetAsync(requestUri, ct), cancellationToken).ConfigureAwait(false);
            await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return response;
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
            var response = await SendWithRetryAsync(ct => _client.PostAsync(requestUri, content, ct), cancellationToken).ConfigureAwait(false);
            await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return response;
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
            var response = await SendWithRetryAsync(ct => _client.PutAsync(requestUri, content, ct), cancellationToken).ConfigureAwait(false);
            await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return response;
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
            var response = await SendWithRetryAsync(ct => _client.DeleteAsync(requestUri, ct), cancellationToken).ConfigureAwait(false);
            await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
            return response;
        } finally {
            _throttle?.Release();
        }
    }

    private async Task<HttpResponseMessage> SendWithRetryAsync(
        Func<CancellationToken, Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken) {
        var attempt = 0;
        var delay = _retryInitialDelay;
        while (true) {
            var response = await send(cancellationToken).ConfigureAwait(false);
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

            if (DelayAsync is null) {
                await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
            } else {
                await DelayAsync(wait, cancellationToken).ConfigureAwait(false);
            }

            attempt++;
            delay = TimeSpan.FromSeconds(delay.TotalSeconds * 2);
        }
    }

    private void ConfigureHeaders(ApiConfig cfg) {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _client.DefaultRequestHeaders.Add("customerUri", cfg.CustomerUri);

        if (cfg.Token is not null && cfg.Token.Length > 0) {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", cfg.Token);
        } else {
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

        _client.Dispose();
        _refreshLock.Dispose();
        _disposed = true;
    }
}
