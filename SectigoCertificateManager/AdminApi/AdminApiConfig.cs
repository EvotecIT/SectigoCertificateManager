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
    public string BaseUrl { get; } = ValidateHttpsUrl(baseUrl, nameof(baseUrl));

    /// <summary>Gets the OAuth2 token endpoint URL.</summary>
    public string TokenUrl { get; } = ValidateHttpsUrl(tokenUrl, nameof(tokenUrl));

    /// <summary>Gets the OAuth2 client identifier.</summary>
    public string ClientId { get; } = string.IsNullOrWhiteSpace(clientId)
        ? throw new ArgumentNullException(nameof(clientId))
        : clientId;

    /// <summary>
    /// Gets the OAuth2 client secret.
    /// <para>
    /// SECURITY NOTE: The secret is stored as a plain string in process memory.
    /// For production use, consider storing credentials in a dedicated secret management
    /// service (for example, Azure Key Vault or AWS Secrets Manager) and rotating them regularly.
    /// </para>
    /// </summary>
    public string ClientSecret { get; } = string.IsNullOrWhiteSpace(clientSecret)
        ? throw new ArgumentNullException(nameof(clientSecret))
        : clientSecret;

    private static string ValidateHttpsUrl(string url, string paramName) {
        if (string.IsNullOrWhiteSpace(url)) {
            throw new ArgumentNullException(paramName);
        }

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri)) {
            throw new ArgumentException($"Invalid URL format: {url}", paramName);
        }

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase)) {
            throw new ArgumentException(
                $"URL must use HTTPS for secure credential transmission: {url}",
                paramName);
        }

        return url;
    }
}
