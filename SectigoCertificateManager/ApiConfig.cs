namespace SectigoCertificateManager;

using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides configuration settings for communicating with the Sectigo Certificate Manager API.
/// </summary>
/// <param name="baseUrl">Base URL of the API endpoint.</param>
/// <param name="username">User account used for authentication.</param>
/// <param name="password">Password associated with <paramref name="username"/>.</param>
/// <param name="customerUri">Value for the <c>customerUri</c> HTTP header.</param>
/// <param name="apiVersion">Version of the API to use.</param>
/// <param name="clientCertificate">Optional client certificate used for mutual TLS.</param>
/// <param name="configureHandler">Optional delegate used to configure the <see cref="HttpClientHandler"/> created by <see cref="SectigoClient"/>.</param>
/// <param name="token">Optional bearer token used for authentication.</param>
/// <param name="tokenExpiresAt">Optional expiration time for <paramref name="token"/>.</param>
/// <param name="refreshToken">Optional delegate used to refresh the token.</param>
public sealed class ApiConfig(
    string baseUrl,
    string username,
    string password,
    string customerUri,
    ApiVersion apiVersion,
    X509Certificate2? clientCertificate = null,
    Action<HttpClientHandler>? configureHandler = null,
    string? token = null,
    DateTimeOffset? tokenExpiresAt = null,
    Func<CancellationToken, Task<TokenInfo>>? refreshToken = null,
    TimeSpan? tokenRefreshThreshold = null,
    int? concurrencyLimit = null,
    int retryCount = 5,
    TimeSpan? retryInitialDelay = null) {
    /// <summary>Gets the base URL of the API endpoint.</summary>
    public string BaseUrl { get; } = baseUrl;

    /// <summary>Gets the user name used for authentication.</summary>
    public string Username { get; } = username;

    /// <summary>Gets the password associated with the <see cref="Username"/> property.</summary>
    public string Password { get; } = password;

    /// <summary>Gets the customer URI part used for API calls.</summary>
    public string CustomerUri { get; } = customerUri;

    /// <summary>Gets the API version that should be used.</summary>
    public ApiVersion ApiVersion { get; } = apiVersion;

    /// <summary>Gets the client certificate used for mutual TLS, if any.</summary>
    public X509Certificate2? ClientCertificate { get; } = clientCertificate;

    /// <summary>Gets the optional handler configuration delegate.</summary>
    public Action<HttpClientHandler>? ConfigureHandler { get; } = configureHandler;

    /// <summary>Gets the bearer token used for authentication, if any.</summary>
    public string? Token { get; } = token;

    /// <summary>Gets the token expiration time, if any.</summary>
    public DateTimeOffset? TokenExpiresAt { get; } = tokenExpiresAt;

    /// <summary>Gets the delegate used to refresh the token, if any.</summary>
    public Func<CancellationToken, Task<TokenInfo>>? RefreshToken { get; } = refreshToken;

    /// <summary>Gets the threshold before expiration when the token should be refreshed.</summary>
    public TimeSpan TokenRefreshThreshold { get; } = tokenRefreshThreshold ?? TimeSpan.FromMinutes(1);

    /// <summary>Gets the optional concurrency limit for HTTP requests.</summary>
    public int? ConcurrencyLimit { get; } = concurrencyLimit;

    /// <summary>Gets the maximum number of retry attempts for transient failures.</summary>
    public int RetryCount { get; } = retryCount;

    /// <summary>Gets the initial delay used for exponential backoff when retrying.</summary>
    public TimeSpan RetryInitialDelay { get; } = retryInitialDelay ?? TimeSpan.FromSeconds(1);
}