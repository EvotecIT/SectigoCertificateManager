namespace SectigoCertificateManager;

using System;
using System.Net.Http;
using System.Net.Http.Headers;
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
        _client.BaseAddress = new Uri(config.BaseUrl);
        _refreshToken = config.RefreshToken;
        _token = config.Token;
        _tokenExpiresAt = config.TokenExpiresAt;
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
        var response = await _client.GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
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
        var response = await _client.PostAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
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
        var response = await _client.PutAsync(requestUri, content, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
    }

    /// <summary>
    /// Sends a DELETE request to the specified endpoint.
    /// </summary>
    /// <param name="requestUri">Relative request URI.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default) {
        ThrowIfDisposed();
        await EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);
        var response = await _client.DeleteAsync(requestUri, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response).ConfigureAwait(false);
        return response;
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
        if (_refreshToken is null) {
            return;
        }

        if (_token is not null && _tokenExpiresAt is not null && _tokenExpiresAt > DateTimeOffset.UtcNow) {
            return;
        }

        await _refreshLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try {
            if (_token is not null && _tokenExpiresAt is not null && _tokenExpiresAt > DateTimeOffset.UtcNow) {
                return;
            }

            var info = await _refreshToken(cancellationToken).ConfigureAwait(false);
            _token = info.Token;
            _tokenExpiresAt = info.ExpiresAt;
            _client.DefaultRequestHeaders.Remove("login");
            _client.DefaultRequestHeaders.Remove("password");
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", info.Token);
        } finally {
            _refreshLock.Release();
        }
    }

    /// <inheritdoc />
    public void Dispose() {
        if (_disposed) {
            return;
        }

        _client.Dispose();
        _disposed = true;
    }
}