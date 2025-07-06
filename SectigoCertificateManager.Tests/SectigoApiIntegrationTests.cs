using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class SectigoApiIntegrationTests : IAsyncLifetime {
    private WireMockServer _server = null!;
    private CertificatesClient _certificates = null!;
    private OrdersClient _orders = null!;
    private ProfilesClient _profiles = null!;

    public Task InitializeAsync() {
        _server = WireMockServer.Start();
        var config = new ApiConfig(_server.Url!, "user", "pass", "cst1", ApiVersion.V25_4);
        var client = new SectigoClient(config, new HttpClient());
        _certificates = new CertificatesClient(client);
        _orders = new OrdersClient(client);
        _profiles = new ProfilesClient(client);
        return Task.CompletedTask;
    }

    public Task DisposeAsync() {
        _server.Stop();
        _server.Dispose();
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CertificatesClient_Get_ReturnsCertificate() {
        _server.Given(Request.Create().WithPath("/v1/certificate/1").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\":1}"));

        var result = await _certificates.GetAsync(1);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task CertificatesClient_Issue_ReturnsCertificate() {
        _server.Given(Request.Create().WithPath("/v1/certificate/issue").UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\":2}"));

        var request = new IssueCertificateRequest { CommonName = "example.com", ProfileId = 1, Term = 12 };
        var result = await _certificates.IssueAsync(request);

        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
    }

    [Fact]
    public async Task OrdersClient_Get_ReturnsOrder() {
        _server.Given(Request.Create().WithPath("/v1/order/5").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("{\"id\":5,\"status\":0,\"orderNumber\":1,\"backendCertId\":\"abc\"}"));

        var result = await _orders.GetAsync(5);

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
    }

    [Fact]
    public async Task OrdersClient_List_ReturnsOrders() {
        _server.Given(Request.Create().WithPath("/v1/order").UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[{\"id\":3,\"status\":0,\"orderNumber\":2,\"backendCertId\":\"def\"}]"));

        var result = await _orders.ListOrdersAsync();

        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal(3, result[0].Id);
    }

    [Fact]
    public async Task OrdersClient_Cancel_Succeeds() {
        _server.Given(Request.Create().WithPath("/v1/order/7/cancel").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(204));

        await _orders.CancelAsync(7);
    }

    [Fact]
    public async Task OrdersClient_Enumerate_ReturnsOrders() {
        _server.Given(Request.Create().WithPath("/v1/order").UsingGet())
            .InScenario("orders")
            .WillSetStateTo("page2")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[{\"id\":1,\"status\":0,\"orderNumber\":1,\"backendCertId\":\"a\"}]"));

        _server.Given(Request.Create().WithPath("/v1/order").UsingGet())
            .InScenario("orders")
            .WhenStateIs("page2")
            .WillSetStateTo("done")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[{\"id\":2,\"status\":0,\"orderNumber\":2,\"backendCertId\":\"b\"}]"));

        _server.Given(Request.Create().WithPath("/v1/order").UsingGet())
            .InScenario("orders")
            .WhenStateIs("done")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody("[]"));

        var list = new List<Order>();
        await foreach (var order in _orders.EnumerateOrdersAsync(pageSize: 1)) {
            list.Add(order);
        }

        Assert.Equal(2, list.Count);
    }
}