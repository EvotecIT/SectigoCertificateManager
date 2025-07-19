namespace SectigoCertificateManager;

using System;
using System.IO;
using System.Text.Json;

/// <summary>
/// Provides helpers for loading <see cref="ApiConfig"/> from environment variables or a JSON file.
/// </summary>
public static class ApiConfigLoader {
    private sealed class TokenFileModel {
        public string Token { get; set; } = string.Empty;
        public DateTimeOffset ExpiresAt { get; set; }
    }
    private sealed class FileModel {
        /// <summary>Gets or sets the base URL.</summary>
        public string BaseUrl { get; set; } = string.Empty;

        /// <summary>Gets or sets the username.</summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>Gets or sets the password.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>Gets or sets the bearer token.</summary>
        public string? Token { get; set; }

        /// <summary>Gets or sets the customer URI.</summary>
        public string CustomerUri { get; set; } = string.Empty;

        /// <summary>Gets or sets the API version string.</summary>
        public string ApiVersion { get; set; } = "V25_6";
    }

    private static string GetTokenPath(string? path) {
        if (!string.IsNullOrEmpty(path)) {
            return path;
        }

        var env = Environment.GetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH");
        if (!string.IsNullOrEmpty(env)) {
            return env;
        }

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".sectigo", "token.json");
    }

    public static TokenInfo? ReadToken(string? path = null) {
        var tokenPath = GetTokenPath(path);
        if (!File.Exists(tokenPath)) {
            return null;
        }

        using var stream = File.OpenRead(tokenPath);
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        var model = JsonSerializer.Deserialize<TokenFileModel>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return model is null ? null : new TokenInfo(model.Token, model.ExpiresAt);
    }

    public static void WriteToken(TokenInfo info, string? path = null) {
        var tokenPath = GetTokenPath(path);
        var dir = Path.GetDirectoryName(tokenPath);
        if (!string.IsNullOrEmpty(dir)) {
            Directory.CreateDirectory(dir);
        }

        var json = JsonSerializer.Serialize(new TokenFileModel { Token = info.Token, ExpiresAt = info.ExpiresAt });
        using (var stream = new FileStream(tokenPath, FileMode.Create, FileAccess.Write, FileShare.None)) {
            using var writer = new StreamWriter(stream);
            writer.Write(json);
        }
#if NET6_0_OR_GREATER
        if (!OperatingSystem.IsWindows()) {
            File.SetUnixFileMode(tokenPath, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
#endif
    }

    /// <summary>
    /// Loads configuration from environment variables or a JSON file.
    /// </summary>
    /// <param name="path">Optional path to the JSON file. If not provided, defaults are used.</param>
    /// <param name="tokenPath">Optional path to the token cache file.</param>
    public static ApiConfig Load(string? path = null, string? tokenPath = null) {
        string? baseUrl = Environment.GetEnvironmentVariable("SECTIGO_BASE_URL");
        string? username = Environment.GetEnvironmentVariable("SECTIGO_USERNAME");
        string? password = Environment.GetEnvironmentVariable("SECTIGO_PASSWORD");
        string? token = Environment.GetEnvironmentVariable("SECTIGO_TOKEN");
        string? customerUri = Environment.GetEnvironmentVariable("SECTIGO_CUSTOMER_URI");
        string? version = Environment.GetEnvironmentVariable("SECTIGO_API_VERSION");

        if (baseUrl is not null && token is not null && customerUri is not null) {
            return new ApiConfig(baseUrl, string.Empty, string.Empty, customerUri, ApiVersionHelper.Parse(version), token: token);
        }

        var cached = ReadToken(tokenPath);

        if (baseUrl is not null && cached is not null && customerUri is not null) {
            return new ApiConfig(baseUrl, string.Empty, string.Empty, customerUri, ApiVersionHelper.Parse(version), token: cached.Token, tokenExpiresAt: cached.ExpiresAt);
        }

        if (baseUrl is not null && username is not null && password is not null && customerUri is not null) {
            return new ApiConfig(baseUrl, username, password, customerUri, ApiVersionHelper.Parse(version));
        }

        if (string.IsNullOrEmpty(path)) {
            var envPath = Environment.GetEnvironmentVariable("SECTIGO_CREDENTIALS_PATH");
            if (!string.IsNullOrEmpty(envPath)) {
                path = envPath;
            } else {
                var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                path = Path.Combine(home, ".sectigo", "credentials.json");
            }
        }

        if (!File.Exists(path!)) {
            throw new FileNotFoundException($"Configuration file not found: {path}", path);
        }

        using var stream = File.OpenRead(path!);
        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var model = JsonSerializer.Deserialize<FileModel>(json, options)
                    ?? throw new InvalidOperationException("Invalid configuration file.");

        var cache = ReadToken(tokenPath);
        var tokenValue = model.Token ?? cache?.Token;
        DateTimeOffset? expires = cache?.ExpiresAt;

        return new ApiConfig(
            model.BaseUrl,
            model.Username,
            model.Password,
            model.CustomerUri,
            ApiVersionHelper.Parse(model.ApiVersion),
            token: tokenValue,
            tokenExpiresAt: expires);
    }
}