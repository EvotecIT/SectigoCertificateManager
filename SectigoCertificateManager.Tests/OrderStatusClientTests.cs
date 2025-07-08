using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="OrderStatusClient"/>.
/// </summary>
public sealed class OrderStatusClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            return Task.FromResult(_response);
        }
    }

    /// <summary>Fetches order status.</summary>
    [Fact]
    public async Task GetStatusAsync_ReturnsOrderStatus() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new { Status = "Submitted" })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var statuses = new OrderStatusClient(client);

        var result = await statuses.GetStatusAsync(5);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/order/5/status", handler.Request!.RequestUri!.ToString());
        Assert.Equal(OrderStatus.Submitted, result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task GetStatusAsync_InvalidOrderId_Throws(int orderId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var statuses = new OrderStatusClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => statuses.GetStatusAsync(orderId));
    }
}
