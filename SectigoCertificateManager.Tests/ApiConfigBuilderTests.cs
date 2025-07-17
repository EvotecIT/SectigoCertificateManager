using SectigoCertificateManager;
using System;
using Xunit;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="ApiConfigBuilder"/>.
/// </summary>
public sealed class ApiConfigBuilderTests {
    /// <summary>Fails when base URL is missing.</summary>
    [Fact]
    public void BuildThrowsWithoutBaseUrl() {
        var builder = new ApiConfigBuilder()
            .WithCredentials("user", "pass")
            .WithCustomerUri("cst1");

        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void BuildThrowsWithoutCredentialsOrToken() {
        var builder = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCustomerUri("cst1");

        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void BuildThrowsWithoutCustomerUri() {
        var builder = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCredentials("user", "pass");

        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void BuildSucceedsWithToken() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCustomerUri("cst1")
            .WithToken("tok")
            .Build();

        Assert.Equal("tok", config.Token);
        Assert.Equal("https://example.com", config.BaseUrl);
    }

    [Fact]
    public void WithBaseUrl_ThrowsForInvalidUri() {
        var builder = new ApiConfigBuilder();

        Assert.Throws<ArgumentException>(() => builder.WithBaseUrl("not a url"));
    }

    [Fact]
    public void WithBaseUrl_AllowsValidUri() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithToken("tok")
            .WithCustomerUri("cst1")
            .Build();

        Assert.Equal("https://example.com", config.BaseUrl);
    }

    [Fact]
    public void BuilderIncludesTokenMetadata() {
        DateTimeOffset expires = DateTimeOffset.UtcNow.AddMinutes(5);
        Func<CancellationToken, Task<TokenInfo>> del = _ => Task.FromResult(new TokenInfo("tok", expires));

        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCustomerUri("cst1")
            .WithToken("tok")
            .WithTokenExpiration(expires)
            .WithTokenRefresh(del)
            .Build();

        Assert.Equal(expires, config.TokenExpiresAt);
        Assert.Same(del, config.RefreshToken);
    }

    [Fact]
    public void WithHttpClientHandler_ThrowsForNullDelegate() {
        var builder = new ApiConfigBuilder();
        Assert.Throws<ArgumentNullException>(() => builder.WithHttpClientHandler(null!));
    }

    [Fact]
    public void WithHttpClientHandler_ThrowsWithCorrectParameterName() {
        var builder = new ApiConfigBuilder();

        var ex = Assert.Throws<ArgumentNullException>(() => builder.WithHttpClientHandler(null!));
        Assert.Equal("configure", ex.ParamName);
    }

    [Fact]
    public void WithCredentials_ThrowsForNullUsername() {
        var builder = new ApiConfigBuilder();

        Assert.Throws<ArgumentException>(() => builder.WithCredentials(null!, "pass"));
    }

    [Fact]
    public void WithCredentials_ThrowsForNullPassword() {
        var builder = new ApiConfigBuilder();

        Assert.Throws<ArgumentException>(() => builder.WithCredentials("user", null!));
    }

    [Fact]
    public async Task WithVersionFromServerAsync_SetsApiVersion() {
        using var server = WireMockServer.Start();
        server.Given(Request.Create().WithPath("/version").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody("25.5"));

        var builder = new ApiConfigBuilder()
            .WithBaseUrl(server.Url!)
            .WithToken("tok")
            .WithCustomerUri("cst1");

        await builder.WithVersionFromServerAsync();
        var config = builder.Build();

        Assert.Equal(ApiVersion.V25_5, config.ApiVersion);
    }
}
