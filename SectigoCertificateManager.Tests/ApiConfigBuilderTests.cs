using SectigoCertificateManager;
using System;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class ApiConfigBuilderTests
{
    [Fact]
    public void BuildThrowsWithoutBaseUrl()
    {
        var builder = new ApiConfigBuilder()
            .WithCredentials("user", "pass")
            .WithCustomerUri("cst1");

        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void BuildThrowsWithoutCredentials()
    {
        var builder = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCustomerUri("cst1");

        Assert.Throws<ArgumentException>(() => builder.Build());
    }

    [Fact]
    public void BuildThrowsWithoutCustomerUri()
    {
        var builder = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCredentials("user", "pass");

        Assert.Throws<ArgumentException>(() => builder.Build());
    }
}
