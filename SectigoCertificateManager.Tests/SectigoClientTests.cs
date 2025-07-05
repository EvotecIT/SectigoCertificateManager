using SectigoCertificateManager;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class SectigoClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        public HttpRequestMessage? Request { get; private set; }
        public CancellationToken Token { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            Token = cancellationToken;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task AddsHeadersAndUsesBaseUrl_WithCredentials() {
        var config = new ApiConfig("https://example.com/api/", "user", "pass", "cst1", ApiVersion.V25_4);
        var handler = new TestHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        await client.GetAsync("v1/test");

        Assert.Equal(new Uri("https://example.com/api/v1/test"), handler.Request!.RequestUri);
        Assert.Equal("user", httpClient.DefaultRequestHeaders.GetValues("login").Single());
        Assert.Equal("pass", httpClient.DefaultRequestHeaders.GetValues("password").Single());
        Assert.Equal("cst1", httpClient.DefaultRequestHeaders.GetValues("customerUri").Single());
    }

    [Fact]
    public async Task AddsBearerHeaderWhenTokenPresent() {
        var config = new ApiConfig("https://example.com/api/", string.Empty, string.Empty, "cst1", ApiVersion.V25_4, token: "tkn");
        var handler = new TestHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        await client.GetAsync("v1/test");

        Assert.True(httpClient.DefaultRequestHeaders.Authorization?.Scheme == "Bearer");
        Assert.Equal("tkn", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
        Assert.Equal("cst1", httpClient.DefaultRequestHeaders.GetValues("customerUri").Single());
        Assert.False(httpClient.DefaultRequestHeaders.Contains("login"));
        Assert.False(httpClient.DefaultRequestHeaders.Contains("password"));
    }

    [Fact]
    public async Task TokenOverridesCredentialsWhenBothProvided() {
        var config = new ApiConfig("https://example.com/api/", "user", "pass", "cst1", ApiVersion.V25_4, token: "tkn");
        var handler = new TestHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        await client.GetAsync("v1/test");

        Assert.True(httpClient.DefaultRequestHeaders.Authorization?.Scheme == "Bearer");
        Assert.Equal("tkn", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
        Assert.False(httpClient.DefaultRequestHeaders.Contains("login"));
        Assert.False(httpClient.DefaultRequestHeaders.Contains("password"));
    }

    [Fact]
    public void ApiConfigBuilderCreatesValidConfig() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCredentials("user", "pass")
            .WithCustomerUri("cst1")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal("user", config.Username);
        Assert.Equal("pass", config.Password);
        Assert.Equal("cst1", config.CustomerUri);
        Assert.Equal(ApiVersion.V25_6, config.ApiVersion);
    }

    [Fact]
    public void BuilderAllowsCertificateAndHandler() {
#pragma warning disable SYSLIB0057
        using var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(Array.Empty<byte>());
#pragma warning restore SYSLIB0057
        System.Net.Http.HttpClientHandler? captured = null;
        System.Action<System.Net.Http.HttpClientHandler> action = h => captured = h;

        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCredentials("user", "pass")
            .WithCustomerUri("cst1")
            .WithClientCertificate(cert)
            .WithHttpClientHandler(action)
            .Build();

        Assert.Same(cert, config.ClientCertificate);
        Assert.Same(action, config.ConfigureHandler);
    }

    [Fact]
    public async Task UsesProvidedCancellationToken() {
        var config = new ApiConfig("https://example.com/api/", "user", "pass", "cst1", ApiVersion.V25_4);
        var handler = new TestHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        try {
            await client.GetAsync("v1/test", cts.Token);
        } catch (TaskCanceledException) {
            // expected on some frameworks
        }

        Assert.True(handler.Token.IsCancellationRequested);
    }
}