namespace SectigoCertificateManager;

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides a builder for creating instances of <see cref="ApiConfig"/> using a fluent API.
/// </summary>
public sealed class ApiConfigBuilder {
    private string _baseUrl = string.Empty;
    private string _username = string.Empty;
    private string _password = string.Empty;
    private string? _token;
    private DateTimeOffset? _tokenExpiresAt;
    private Func<CancellationToken, Task<TokenInfo>>? _refreshToken;
    private string _customerUri = string.Empty;
    private ApiVersion _apiVersion = ApiVersion.V25_6;
    private X509Certificate2? _clientCertificate;
    private Action<HttpClientHandler>? _configureHandler;
    private int? _concurrencyLimit;

    /// <summary>Sets the base URL for the API endpoint.</summary>
    /// <param name="baseUrl">The root URL of the Sectigo API.</param>
    public ApiConfigBuilder WithBaseUrl(string baseUrl) {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out _)) {
            throw new ArgumentException("Base URL must be a valid absolute URI.", nameof(baseUrl));
        }

        _baseUrl = baseUrl;
        return this;
    }

    /// <summary>Sets the credentials used for authentication.</summary>
    /// <param name="username">User name for API authentication.</param>
    /// <param name="password">Password associated with <paramref name="username"/>.</param>
    public ApiConfigBuilder WithCredentials(string username, string password) {
        _username = username;
        _password = password;
        return this;
    }

    /// <summary>Sets the bearer token used for authentication.</summary>
    /// <param name="token">Token value.</param>
    public ApiConfigBuilder WithToken(string token) {
        _token = token;
        return this;
    }

    /// <summary>Sets the token expiration time.</summary>
    /// <param name="expiresAt">UTC time when the token expires.</param>
    public ApiConfigBuilder WithTokenExpiration(DateTimeOffset expiresAt) {
        _tokenExpiresAt = expiresAt;
        return this;
    }

    /// <summary>Sets the delegate used to refresh the token when expired.</summary>
    /// <param name="refresh">Delegate invoked to obtain a new token.</param>
    public ApiConfigBuilder WithTokenRefresh(Func<CancellationToken, Task<TokenInfo>> refresh) {
        _refreshToken = refresh;
        return this;
    }

    /// <summary>Sets the customer URI header value.</summary>
    /// <param name="customerUri">Value of the <c>customerUri</c> header.</param>
    public ApiConfigBuilder WithCustomerUri(string customerUri) {
        _customerUri = customerUri;
        return this;
    }

    /// <summary>Sets the API version.</summary>
    /// <param name="version">Desired API version.</param>
    public ApiConfigBuilder WithApiVersion(ApiVersion version) {
        _apiVersion = version;
        return this;
    }

    /// <summary>Attaches a client certificate for mutual TLS authentication.</summary>
    /// <param name="certificate">The certificate used for client authentication.</param>
    public ApiConfigBuilder WithClientCertificate(X509Certificate2 certificate) {
        _clientCertificate = certificate;
        return this;
    }

    /// <summary>Allows configuration of the <see cref="HttpClientHandler"/> used by <see cref="SectigoClient"/>.</summary>
    /// <param name="configure">Delegate used to configure the handler.</param>
    public ApiConfigBuilder WithHttpClientHandler(Action<HttpClientHandler> configure) {
        _configureHandler = configure ?? throw new ArgumentNullException(nameof(configure));
        return this;
    }

    /// <summary>Limits the number of concurrent HTTP requests.</summary>
    /// <param name="limit">Maximum number of simultaneous requests.</param>
    public ApiConfigBuilder WithConcurrencyLimit(int limit) {
        if (limit <= 0) {
            throw new ArgumentOutOfRangeException(nameof(limit));
        }

        _concurrencyLimit = limit;
        return this;
    }

    /// <summary>Builds a new <see cref="ApiConfig"/> instance using configured values.</summary>
    public ApiConfig Build() {
        if (string.IsNullOrWhiteSpace(_baseUrl)) {
            throw new ArgumentException("Base URL is required.", nameof(_baseUrl));
        }

        var hasToken = !string.IsNullOrWhiteSpace(_token);
        var hasCredentials = !string.IsNullOrWhiteSpace(_username) && !string.IsNullOrWhiteSpace(_password);

        if (!hasToken && !hasCredentials) {
            throw new ArgumentException("Credentials or token are required.");
        }

        if (!hasToken) {
            if (string.IsNullOrWhiteSpace(_username)) {
                throw new ArgumentException("User name is required.", nameof(_username));
            }

            if (string.IsNullOrWhiteSpace(_password)) {
                throw new ArgumentException("Password is required.", nameof(_password));
            }
        }

        if (string.IsNullOrWhiteSpace(_customerUri)) {
            throw new ArgumentException("Customer URI is required.", nameof(_customerUri));
        }

        return new ApiConfig(
            _baseUrl,
            _username,
            _password,
            _customerUri,
            _apiVersion,
            _clientCertificate,
            _configureHandler,
            _token,
            _tokenExpiresAt,
            _refreshToken,
            _concurrencyLimit);
    }
}