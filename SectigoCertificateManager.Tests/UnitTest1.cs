using SectigoCertificateManager;
using System.Linq;
using System.Net.Http;
using Xunit;

namespace SectigoCertificateManager.Tests;

public class UnitTest1
{
    [Fact]
    public void Test1()
    {
        var obj = new Class1();
        Assert.Equal("Class1", obj.Name);
    }

    [Fact]
    public void SectigoClientAddsDefaultHeaders()
    {
        var config = new ApiConfig("https://example.com", "user", "pass", "cst1", ApiVersion.V25_4);
        var httpClient = new HttpClient();
        var client = new SectigoClient(config, httpClient);

        Assert.True(httpClient.DefaultRequestHeaders.Contains("login"));
        Assert.True(httpClient.DefaultRequestHeaders.Contains("password"));
        Assert.True(httpClient.DefaultRequestHeaders.Contains("customerUri"));

        Assert.Equal("user", httpClient.DefaultRequestHeaders.GetValues("login").Single());
        Assert.Equal("pass", httpClient.DefaultRequestHeaders.GetValues("password").Single());
        Assert.Equal("cst1", httpClient.DefaultRequestHeaders.GetValues("customerUri").Single());
    }

    [Fact]
    public void ApiConfigBuilderCreatesValidConfig()
    {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCredentials("user", "pass")
            .WithCustomerUri("cst1")
            .WithApiVersion(ApiVersion.V25_5)
            .Build();

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal("user", config.Username);
        Assert.Equal("pass", config.Password);
        Assert.Equal("cst1", config.CustomerUri);
    Assert.Equal(ApiVersion.V25_5, config.ApiVersion);
    }

    [Fact]
    public void BuilderAddsHandlerConfiguration()
    {
        void Configure(HttpClientHandler handler)
        {
            handler.AllowAutoRedirect = false;
        }

        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCredentials("user", "pass")
            .WithCustomerUri("cst1")
            .WithHandlerConfiguration(Configure)
            .Build();

        var handler = new HttpClientHandler { AllowAutoRedirect = true };
        config.ConfigureHandler?.Invoke(handler);
        Assert.False(handler.AllowAutoRedirect);
    }
}
