using SectigoCertificateManager;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="SectigoClient"/>.
/// </summary>
public sealed class SectigoClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private sealed class DisposableHandler : HttpMessageHandler {
        public bool Disposed { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            Disposed = true;
        }
    }

    /// <summary>Uses credentials to add HTTP headers.</summary>
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
    public void DisposeDisposesHttpClient() {
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);
        var handler = new DisposableHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        client.Dispose();

        Assert.True(handler.Disposed);
    }

    [Fact]
    public void DisposeIsIdempotent() {
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);
        var handler = new DisposableHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        client.Dispose();
        client.Dispose();

        Assert.True(handler.Disposed);
    }

    [Fact]
    public async Task MethodsThrowAfterDispose() {
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);
        var handler = new TestHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        client.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(() => client.GetAsync("v1/test"));
    }

    [Fact]
    public async Task RefreshesTokenAutomatically() {
        var expired = DateTimeOffset.UtcNow.AddMinutes(-1);
        var called = false;
        Task<TokenInfo> Refresh(CancellationToken ct) {
            called = true;
            return Task.FromResult(new TokenInfo("new", DateTimeOffset.UtcNow.AddMinutes(30)));
        }

        var config = new ApiConfig(
            "https://example.com/",
            string.Empty,
            string.Empty,
            "c",
            ApiVersion.V25_4,
            token: "old",
            tokenExpiresAt: expired,
            refreshToken: Refresh);
        var handler = new TestHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        await client.GetAsync("v1/test");

        Assert.True(called);
        Assert.Equal("new", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    [Fact]
    public async Task DoesNotRefreshValidToken() {
        var expires = DateTimeOffset.UtcNow.AddMinutes(10);
        var called = false;
        Task<TokenInfo> Refresh(CancellationToken ct) {
            called = true;
            return Task.FromResult(new TokenInfo("new", DateTimeOffset.UtcNow.AddMinutes(30)));
        }

        var config = new ApiConfig(
            "https://example.com/",
            string.Empty,
            string.Empty,
            "c",
            ApiVersion.V25_4,
            token: "old",
            tokenExpiresAt: expires,
            refreshToken: Refresh);
        var handler = new TestHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);

        await client.GetAsync("v1/test");

        Assert.False(called);
        Assert.Equal("old", httpClient.DefaultRequestHeaders.Authorization?.Parameter);
    }

    private sealed class ThrottleHandler : HttpMessageHandler {
        private readonly TimeSpan _delay;
        private int _current;

        public int MaxObserved { get; private set; }

        public ThrottleHandler(TimeSpan delay) {
            _delay = delay;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            var current = Interlocked.Increment(ref _current);
            if (current > MaxObserved) {
                MaxObserved = current;
            }

            await Task.Delay(_delay, cancellationToken);
            Interlocked.Decrement(ref _current);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }
    }

    [Fact]
    public async Task ConcurrencyIsLimited() {
        var handler = new ThrottleHandler(TimeSpan.FromMilliseconds(50));
        var httpClient = new HttpClient(handler);
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com/")
            .WithCredentials("u", "p")
            .WithCustomerUri("c")
            .WithConcurrencyLimit(2)
            .Build();

        var client = new SectigoClient(config, httpClient);

        await Task.WhenAll(
            client.GetAsync("v1/a"),
            client.GetAsync("v1/b"),
            client.GetAsync("v1/c"));

        Assert.True(handler.MaxObserved <= 2);
    }
}