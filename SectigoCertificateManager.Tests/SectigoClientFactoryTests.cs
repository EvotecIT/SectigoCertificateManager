using SectigoCertificateManager;
using System;
using System.Net.Http;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="SectigoClientFactory"/>.
/// </summary>
public sealed class SectigoClientFactoryTests {
    [Fact]
    public void Create_WithHttpClient_ReturnsClient() {
        var factory = new SectigoClientFactory();
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);
        var httpClient = new HttpClient();

        using var client = factory.Create(config, httpClient);

        Assert.IsType<SectigoClient>(client);
        Assert.Same(httpClient, client.HttpClient);
    }

    [Fact]
    public void Create_WithoutHttpClient_ConfiguresBaseAddress() {
        var factory = new SectigoClientFactory();
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);

        using var client = factory.Create(config);

        Assert.Equal(new Uri("https://example.com/"), client.HttpClient.BaseAddress);
    }
}