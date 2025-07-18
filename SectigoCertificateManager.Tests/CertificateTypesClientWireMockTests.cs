using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace SectigoCertificateManager.Tests;

#if !NET472
/// <summary>
/// Integration tests for <see cref="CertificateTypesClient"/> using WireMock.
/// </summary>
public sealed class CertificateTypesClientWireMockTests {
    [Fact]
    public async Task UpsertAsync_PostsWhenIdMissing() {
        using var server = WireMockServer.Start();
        server.Given(Request.Create().WithPath("/v1/certificate/type").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\":8}"));

        var config = new ApiConfig(server.Url!, "user", "pass", "cst1", ApiVersion.V25_4);
        var client = new SectigoClient(config, new HttpClient());
        var types = new CertificateTypesClient(client);

        var result = await types.UpsertAsync(new CertificateType { Name = "new" });

        Assert.NotNull(result);
        Assert.Equal(8, result!.Id);
    }

    [Fact]
    public async Task UpsertAsync_PutsWhenIdPresent() {
        using var server = WireMockServer.Start();
        server.Given(Request.Create().WithPath("/v1/certificate/type/3").UsingPut())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\":3,\"name\":\"upd\"}"));

        var config = new ApiConfig(server.Url!, "user", "pass", "cst1", ApiVersion.V25_4);
        var client = new SectigoClient(config, new HttpClient());
        var types = new CertificateTypesClient(client);

        var result = await types.UpsertAsync(new CertificateType { Id = 3, Name = "upd" });

        Assert.NotNull(result);
        Assert.Equal("upd", result!.Name);
    }
}
#endif
