namespace SectigoCertificateManager;

using System;
using System.IO;
using System.Text.Json;

/// <summary>
/// Provides helpers for loading <see cref="ApiConfig"/> from environment variables or a JSON file.
/// </summary>
public static class ApiConfigLoader
{
    private sealed class FileModel
    {
        /// <summary>Gets or sets the base URL.</summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the username.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Gets or sets the password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets the customer URI.</summary>
        public string CustomerUri { get; set; } = string.Empty;

        /// <summary>Gets or sets the API version string.</summary>
        public string ApiVersion { get; set; } = "V25_4";
    }

    /// <summary>
    /// Loads configuration from environment variables or a JSON file.
    /// </summary>
    /// <param name="path">Optional path to the JSON file. If not provided, defaults are used.</param>
    public static ApiConfig Load(string? path = null)
    {
        string? baseUrl = Environment.GetEnvironmentVariable("SECTIGO_BASE_URL");
        string? username = Environment.GetEnvironmentVariable("SECTIGO_USERNAME");
        string? password = Environment.GetEnvironmentVariable("SECTIGO_PASSWORD");
        string? customerUri = Environment.GetEnvironmentVariable("SECTIGO_CUSTOMER_URI");
        string? version = Environment.GetEnvironmentVariable("SECTIGO_API_VERSION");

        if (baseUrl is not null && username is not null && password is not null && customerUri is not null)
        {
            return new ApiConfig(baseUrl, username, password, customerUri, ParseVersion(version));
        }

        if (string.IsNullOrEmpty(path))
        {
            var envPath = Environment.GetEnvironmentVariable("SECTIGO_CREDENTIALS_PATH");
            if (!string.IsNullOrEmpty(envPath))
            {
                path = envPath;
            }
            else
            {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                path = Path.Combine(home, ".sectigo", "credentials.json");
            }
        }

        var json = File.ReadAllText(path!);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var model = JsonSerializer.Deserialize<FileModel>(json, options)
                    ?? throw new InvalidOperationException("Invalid configuration file.");
        return new ApiConfig(model.BaseUrl, model.Username, model.Password, model.CustomerUri, ParseVersion(model.ApiVersion));
    }

    private static ApiVersion ParseVersion(string? value)
        => Enum.TryParse<ApiVersion>(value, ignoreCase: true, out var v) ? v : ApiVersion.V25_4;
}
