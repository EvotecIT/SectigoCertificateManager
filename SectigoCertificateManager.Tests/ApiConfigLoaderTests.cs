using SectigoCertificateManager;
using System;
using System.IO;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="ApiConfigLoader"/>.
/// </summary>
public sealed class ApiConfigLoaderTests {
    /// <summary>Loads configuration from file.</summary>
    [Fact]
    public void Load_FromFile() {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var path = Path.Combine(tempDir, "cred.json");
        File.WriteAllText(path, "{\"baseUrl\":\"https://example.com\",\"username\":\"user\",\"password\":\"pass\",\"customerUri\":\"cst1\",\"apiVersion\":\"V25_6\"}");

        var config = ApiConfigLoader.Load(path);

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Load_FromEnvironment() {
        Environment.SetEnvironmentVariable("SECTIGO_BASE_URL", "https://example.com");
        Environment.SetEnvironmentVariable("SECTIGO_USERNAME", "user");
        Environment.SetEnvironmentVariable("SECTIGO_PASSWORD", "pass");
        Environment.SetEnvironmentVariable("SECTIGO_CUSTOMER_URI", "cst1");
        Environment.SetEnvironmentVariable("SECTIGO_API_VERSION", "V25_4");

        var config = ApiConfigLoader.Load();

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal(ApiVersion.V25_4, config.ApiVersion);

        Environment.SetEnvironmentVariable("SECTIGO_BASE_URL", null);
        Environment.SetEnvironmentVariable("SECTIGO_USERNAME", null);
        Environment.SetEnvironmentVariable("SECTIGO_PASSWORD", null);
        Environment.SetEnvironmentVariable("SECTIGO_CUSTOMER_URI", null);
        Environment.SetEnvironmentVariable("SECTIGO_API_VERSION", null);
    }

    [Fact]
    public void Load_UsesDefaultPathFromEnvironment() {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var path = Path.Combine(tempDir, "cred.json");
        File.WriteAllText(path, "{\"baseUrl\":\"https://example.com\",\"username\":\"user\",\"password\":\"pass\",\"customerUri\":\"cst1\"}");
        Environment.SetEnvironmentVariable("SECTIGO_CREDENTIALS_PATH", path);

        var config = ApiConfigLoader.Load();

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);

        Environment.SetEnvironmentVariable("SECTIGO_CREDENTIALS_PATH", null);
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Load_FromEnvironment_WithToken() {
        Environment.SetEnvironmentVariable("SECTIGO_BASE_URL", "https://example.com");
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN", "tok");
        Environment.SetEnvironmentVariable("SECTIGO_CUSTOMER_URI", "cst1");

        var config = ApiConfigLoader.Load();

        Assert.Equal("tok", config.Token);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);

        Environment.SetEnvironmentVariable("SECTIGO_BASE_URL", null);
        Environment.SetEnvironmentVariable("SECTIGO_TOKEN", null);
        Environment.SetEnvironmentVariable("SECTIGO_CUSTOMER_URI", null);
    }

    [Fact]
    public void Load_FromFile_WithToken() {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var path = Path.Combine(tempDir, "cred.json");
        File.WriteAllText(path, "{\"baseUrl\":\"https://example.com\",\"token\":\"tok\",\"customerUri\":\"cst1\"}");

        var config = ApiConfigLoader.Load(path);

        Assert.Equal("tok", config.Token);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);

        Directory.Delete(tempDir, true);
    }

    [Fact]
    public void Load_WithMissingFile_Throws() {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "missing.json");

        var ex = Assert.Throws<FileNotFoundException>(() => ApiConfigLoader.Load(path));
        Assert.Contains(path, ex.Message);
    }
}