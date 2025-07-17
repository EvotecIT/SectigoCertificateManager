using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
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
    [Theory]
    [MemberData(nameof(StatusCases))]
    public async Task GetStatusAsync_ReturnsOrderStatus(string text, OrderStatus expected) {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new { Status = text })
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var statuses = new OrderStatusClient(client);

        var result = await statuses.GetStatusAsync(5);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/order/5/status", handler.Request!.RequestUri!.ToString());
        Assert.Equal(expected, result);
    }

    private sealed class SequenceHandler : HttpMessageHandler {
        private readonly Queue<HttpResponseMessage> _responses;
        public List<HttpRequestMessage> Requests { get; } = new();

        public SequenceHandler(IEnumerable<HttpResponseMessage> responses) => _responses = new Queue<HttpResponseMessage>(responses);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Requests.Add(request);
            return Task.FromResult(_responses.Dequeue());
        }
    }

    [Fact]
    public async Task WatchAsync_PollsUntilTerminal() {
        var responses = new[] {
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Status = "Submitted" }) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Status = "Submitted" }) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { Status = "Completed" }) }
        };

        var handler = new SequenceHandler(responses);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var statuses = new OrderStatusClient(client);

        var result = await statuses.WatchAsync(3, TimeSpan.Zero);

        Assert.Equal(3, handler.Requests.Count);
        foreach (var req in handler.Requests) {
            Assert.Equal("https://example.com/v1/order/3/status", req.RequestUri!.ToString());
        }
        Assert.Equal(OrderStatus.Completed, result);
    }

    public static IEnumerable<object[]> StatusCases() {
        foreach (OrderStatus status in Enum.GetValues(typeof(OrderStatus))) {
            yield return new object[] { status.ToString(), status };
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task GetStatusAsync_InvalidOrderId_Throws(int orderId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var statuses = new OrderStatusClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => statuses.GetStatusAsync(orderId));
    }
}