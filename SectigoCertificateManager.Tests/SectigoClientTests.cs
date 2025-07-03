<<<<<<< HEAD:SectigoCertificateManager.Tests/UnitTest1.cs
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

=======
using SectigoCertificateManager;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class SectigoClientTests
{
    private sealed class TestHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    [Fact]
    public async Task AddsHeadersAndUsesBaseUrl()
    {
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
    public void ApiConfigBuilderCreatesValidConfig()
    {
>>>>>>> origin/main:SectigoCertificateManager.Tests/SectigoClientTests.cs
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com")
            .WithCredentials("user", "pass")
            .WithCustomerUri("cst1")
<<<<<<< HEAD:SectigoCertificateManager.Tests/UnitTest1.cs
            .WithHandlerConfiguration(Configure)
            .Build();

        var handler = new HttpClientHandler { AllowAutoRedirect = true };
        config.ConfigureHandler?.Invoke(handler);
        Assert.False(handler.AllowAutoRedirect);
=======
            .WithApiVersion(ApiVersion.V25_5)
            .Build();

        Assert.Equal("https://example.com", config.BaseUrl);
        Assert.Equal("user", config.Username);
        Assert.Equal("pass", config.Password);
        Assert.Equal("cst1", config.CustomerUri);
        Assert.Equal(ApiVersion.V25_5, config.ApiVersion);
>>>>>>> origin/main:SectigoCertificateManager.Tests/SectigoClientTests.cs
    }
}
