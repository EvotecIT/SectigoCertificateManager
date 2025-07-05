using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class OrdersClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;
        public CancellationToken Token { get; private set; }
        public HttpRequestMessage? Request { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            Token = cancellationToken;
            return Task.FromResult(_response);
        }
    }

    [Fact]
    public async Task ListOrdersAsync_RequestsOrders() {
        var order = new Order { Id = 1, OrderNumber = 10, BackendCertId = "a" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new[] { order })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var orders = new OrdersClient(client);

        var result = await orders.ListOrdersAsync();

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/order", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task GetAsync_ThrowsApiExceptionOnFailure() {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) {
            Content = JsonContent.Create(new ApiError { Code = -2, Description = "fail" })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var orders = new OrdersClient(client);

        var ex = await Assert.ThrowsAsync<ApiException>(() => orders.GetAsync(7));
        Assert.Equal(-2, ex.ErrorCode);
    }

    [Fact]
    public async Task GetAsync_PassesCancellationToken() {
        var order = new Order { Id = 2 };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(order)
        };
        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var orders = new OrdersClient(client);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<TaskCanceledException>(() => orders.GetAsync(2, cts.Token));
    }
}