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
}
