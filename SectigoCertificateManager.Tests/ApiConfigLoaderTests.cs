using SectigoCertificateManager;
using System;
using System.IO;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="ApiConfigLoader"/>.
/// </summary>
[Collection(ApiConfigEnvironmentCollection.Name)]
public sealed class ApiConfigLoaderTests : IDisposable {
    private readonly ApiConfigEnvironmentScope _environment = new();

    public void Dispose() => _environment.Dispose();

    /// <summary>Loads configuration from file.</summary>
    [Fact]
    public void Load_FromFile() {
        var tempDir = _environment.RootDirectory;
        var path = Path.Combine(tempDir, "cred.json");
        File.WriteAllText(path, "{\"baseUrl\":\"https://example.com\",\"username\":\"user\",\"password\":\"pass\",\"customerUri\":\"cst1\",\"apiVersion\":\"V25_6\"}");

        var config = ApiConfigLoader.Load(path, _environment.TokenCachePath);

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);
    }

    [Fact]
    public void Load_FromEnvironment() {
        var tempDir = _environment.RootDirectory;
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH", Path.Combine(tempDir, "token.json"));
        Environment.SetEnvironmentVariable("SECTIGO_BASE_URL", "https://example.com");
        Environment.SetEnvironmentVariable("SECTIGO_USERNAME", "user");
        Environment.SetEnvironmentVariable("SECTIGO_PASSWORD", "pass");
        Environment.SetEnvironmentVariable("SECTIGO_CUSTOMER_URI", "cst1");
        Environment.SetEnvironmentVariable("SECTIGO_API_VERSION", "V25_4");

        var config = ApiConfigLoader.Load();

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal(ApiVersion.V25_4, config.ApiVersion);
    }

    [Fact]
    public void Load_UsesDefaultPathFromEnvironment() {
        var tempDir = _environment.RootDirectory;
        var path = Path.Combine(tempDir, "cred.json");
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH", Path.Combine(tempDir, "token.json"));
        File.WriteAllText(path, "{\"baseUrl\":\"https://example.com\",\"username\":\"user\",\"password\":\"pass\",\"customerUri\":\"cst1\"}");
        Environment.SetEnvironmentVariable("SECTIGO_CREDENTIALS_PATH", path);

        var config = ApiConfigLoader.Load();

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);
    }

    [Fact]
    public void Load_FromEnvironment_WithToken() {
        var tempDir = _environment.RootDirectory;
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH", Path.Combine(tempDir, "token.json"));
        Environment.SetEnvironmentVariable("SECTIGO_BASE_URL", "https://example.com");
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN", "tok");
        Environment.SetEnvironmentVariable("SECTIGO_CUSTOMER_URI", "cst1");

        var config = ApiConfigLoader.Load();

        Assert.Equal("tok", config.Token);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);
    }

    [Fact]
    public void Load_FromFile_WithToken() {
        var tempDir = _environment.RootDirectory;
        var path = Path.Combine(tempDir, "cred.json");
        var tokenPath = Path.Combine(tempDir, "token.json");
        File.WriteAllText(path, "{\"baseUrl\":\"https://example.com\",\"token\":\"tok\",\"customerUri\":\"cst1\"}");

        var config = ApiConfigLoader.Load(path, tokenPath);

        Assert.Equal("tok", config.Token);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);
    }

    [Fact]
    public void Load_WithMissingFile_Throws() {
        var path = Path.Combine(_environment.RootDirectory, "missing.json");

        var ex = Assert.ThrowsAny<IOException>(() => ApiConfigLoader.Load(path));
        Assert.Contains("Configuration file not found", ex.Message);
    }

    [Fact]
    public void TokenCache_Roundtrip() {
        var tempDir = _environment.RootDirectory;
        var path = Path.Combine(tempDir, "token.json");

        var info = new TokenInfo("tok", DateTimeOffset.UtcNow.AddMinutes(10));
        ApiConfigLoader.WriteToken(info, path);

        var loaded = ApiConfigLoader.ReadToken(path);

        Assert.NotNull(loaded);
        Assert.Equal(info.Token, loaded!.Token);
        Assert.Equal(info.ExpiresAt, loaded.ExpiresAt);
    }

    [Fact]
    public void WriteToken_WithNullInfo_Throws() {
        Assert.Throws<ArgumentNullException>(() => ApiConfigLoader.WriteToken(null!));
    }

    [Fact]
    public void Load_FromTokenCache() {
        var tempDir = _environment.RootDirectory;
        var tokenPath = Path.Combine(tempDir, "token.json");
        var info = new TokenInfo("tok", DateTimeOffset.UtcNow.AddMinutes(5));
        ApiConfigLoader.WriteToken(info, tokenPath);

        Environment.SetEnvironmentVariable("SECTIGO_BASE_URL", "https://example.com");
        Environment.SetEnvironmentVariable("SECTIGO_CUSTOMER_URI", "cst1");
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH", tokenPath);

        var config = ApiConfigLoader.Load();

        Assert.Equal("tok", config.Token);
        Assert.Equal(info.ExpiresAt, config.TokenExpiresAt);
    }

    [Fact]
    public void Load_FromTokenCache_IgnoresExpiredToken() {
        var tempDir = _environment.RootDirectory;
        var tokenPath = Path.Combine(tempDir, "token.json");
        var info = new TokenInfo("tok", DateTimeOffset.UtcNow.AddMinutes(-5));
        ApiConfigLoader.WriteToken(info, tokenPath);

        Environment.SetEnvironmentVariable("SECTIGO_BASE_URL", "https://example.com");
        Environment.SetEnvironmentVariable("SECTIGO_USERNAME", "user");
        Environment.SetEnvironmentVariable("SECTIGO_PASSWORD", "pass");
        Environment.SetEnvironmentVariable("SECTIGO_CUSTOMER_URI", "cst1");
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN_CACHE_PATH", tokenPath);

        var config = ApiConfigLoader.Load();

        Assert.Null(config.Token);
        Assert.Null(config.TokenExpiresAt);
        Assert.Equal("user", config.Username);
    }

    [Fact]
    public void Load_FromFile_IgnoresExpiredTokenCache() {
        var tempDir = _environment.RootDirectory;
        var tokenPath = Path.Combine(tempDir, "token.json");
        var configPath = Path.Combine(tempDir, "cred.json");
        var info = new TokenInfo("tok", DateTimeOffset.UtcNow.AddMinutes(-5));
        ApiConfigLoader.WriteToken(info, tokenPath);

        File.WriteAllText(configPath, "{\"baseUrl\":\"https://example.com\",\"username\":\"user\",\"password\":\"pass\",\"customerUri\":\"cst1\"}");

        var config = ApiConfigLoader.Load(configPath, tokenPath);

        Assert.Null(config.Token);
        Assert.Null(config.TokenExpiresAt);
        Assert.Equal("user", config.Username);
    }

    [Fact]
    public void Load_FromFile_UsesValidTokenCache() {
        var tempDir = _environment.RootDirectory;
        var tokenPath = Path.Combine(tempDir, "token.json");
        var configPath = Path.Combine(tempDir, "cred.json");
        var info = new TokenInfo("tok", DateTimeOffset.UtcNow.AddMinutes(10));
        ApiConfigLoader.WriteToken(info, tokenPath);

        File.WriteAllText(configPath, "{\"baseUrl\":\"https://example.com\",\"username\":\"user\",\"password\":\"pass\",\"customerUri\":\"cst1\"}");

        var config = ApiConfigLoader.Load(configPath, tokenPath);

        Assert.Equal("tok", config.Token);
        Assert.Equal(info.ExpiresAt, config.TokenExpiresAt);
    }
}
