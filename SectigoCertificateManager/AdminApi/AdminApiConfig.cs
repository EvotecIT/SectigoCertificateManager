namespace SectigoCertificateManager.AdminApi;

using System;

/// <summary>
/// Configuration for the Sectigo Admin Operations API using OAuth2 client credentials.
/// </summary>
/// <param name="baseUrl">Base URL of the Admin API (for example, https://admin.enterprise.sectigo.com).</param>
/// <param name="tokenUrl">OAuth2 token endpoint URL.</param>
/// <param name="clientId">OAuth2 client identifier.</param>
/// <param name="clientSecret">OAuth2 client secret.</param>
public sealed class AdminApiConfig(
    string baseUrl,
    string tokenUrl,
    string clientId,
    string clientSecret) {
    /// <summary>Gets the base URL of the Admin API.</summary>
    public string BaseUrl { get; } = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));

    /// <summary>Gets the OAuth2 token endpoint URL.</summary>
    public string TokenUrl { get; } = tokenUrl ?? throw new ArgumentNullException(nameof(tokenUrl));

    /// <summary>Gets the OAuth2 client identifier.</summary>
    public string ClientId { get; } = clientId ?? throw new ArgumentNullException(nameof(clientId));

    /// <summary>Gets the OAuth2 client secret.</summary>
    public string ClientSecret { get; } = clientSecret ?? throw new ArgumentNullException(nameof(clientSecret));
}

