using SectigoCertificateManager;
using System;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class ApiConfigBuilderTests {
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
}