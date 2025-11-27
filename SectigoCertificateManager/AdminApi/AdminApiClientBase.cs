namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Shared infrastructure for Admin Operations API clients (HTTP client + token management).
/// </summary>
public abstract class AdminApiClientBase : IDisposable {
    protected readonly AdminApiConfig _config;
    protected readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly AdminTokenManager _tokenManager;

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

        _tokenManager = new AdminTokenManager(_httpClient, _config);
    }

    /// <summary>
    /// Retrieves an OAuth2 access token, using cached value when possible.
    /// </summary>
    protected Task<string> GetAccessTokenAsync(CancellationToken cancellationToken) =>
        _tokenManager.GetTokenAsync(cancellationToken);

    /// <inheritdoc />
    public void Dispose() {
        _tokenManager.Dispose();
        if (_ownsHttpClient) {
            _httpClient.Dispose();
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
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string _cachedToken = string.Empty;
    private DateTimeOffset _tokenExpiresAt;

    public AdminTokenManager(HttpClient httpClient, AdminApiConfig config) {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _config = config ?? throw new ArgumentNullException(nameof(config));
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

            using var content = new FormUrlEncodedContent(new Dictionary<string, string> {
                ["grant_type"] = "client_credentials",
                ["client_id"] = _config.ClientId,
                ["client_secret"] = _config.ClientSecret
            });

            using var response = await _httpClient
                .PostAsync(_config.TokenUrl, content, cancellationToken)
                .ConfigureAwait(false);
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
