namespace SectigoCertificateManager;

/// <summary>
/// Provides configuration settings for communicating with the Sectigo Certificate Manager API.
/// </summary>
/// <param name="baseUrl">Base URL of the API endpoint.</param>
/// <param name="username">User account used for authentication.</param>
/// <param name="password">Password associated with <paramref name="username"/>.</param>
/// <param name="apiVersion">Version of the API to use.</param>
public sealed class ApiConfig(string baseUrl, string username, string password, ApiVersion apiVersion)
{
    /// <summary>
    /// Gets the base URL of the API endpoint.
    /// </summary>
    public string BaseUrl { get; } = baseUrl;

    /// <summary>
    /// Gets the user name used for authentication.
    /// </summary>
    public string Username { get; } = username;

    /// <summary>
    /// Gets the password associated with the <see cref="Username"/> property.
    /// </summary>
    public string Password { get; } = password;

    /// <summary>
    /// Gets the API version that should be used.
    /// </summary>
    public ApiVersion ApiVersion { get; } = apiVersion;
}

/// <summary>
/// Enumerates available Sectigo Certificate Manager API versions.
/// </summary>
public enum ApiVersion
{
    /// <summary>
    /// Version 25.4 of the API.
    /// </summary>
    V25_4,

    /// <summary>
    /// Version 25.5 of the API.
    /// </summary>
    V25_5,
}
