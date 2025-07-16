using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="OrdersClient"/>.
/// </summary>
public sealed class OrdersClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            if (request.Content is not null) {
#if NETSTANDARD2_0 || NET472
                await request.Content.CopyToAsync(Stream.Null).ConfigureAwait(false);
#else
                await request.Content.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);
#endif
            }
            return _response;
        }
    }

    /// <summary>Lists certificate orders.</summary>
    [Fact]
    public async Task ListOrdersAsync_RequestsOrders() {
        var order = new Order { Id = 1, OrderNumber = 10, BackendCertId = "a" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new[] { order })
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var orders = new OrdersClient(client);

        var result = await orders.ListOrdersAsync();

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/order?size=200&position=0", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task CancelAsync_SendsPostRequest() {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var orders = new OrdersClient(client);

        await orders.CancelAsync(5);

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://example.com/v1/order/5/cancel", handler.Request.RequestUri!.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAsync_InvalidOrderId_Throws(int orderId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var orders = new OrdersClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => orders.GetAsync(orderId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task CancelAsync_InvalidOrderId_Throws(int orderId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.NoContent));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var orders = new OrdersClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => orders.CancelAsync(orderId));
    }

    [Fact]
    public async Task GetHistoryAsync_ReturnsEntries() {
        var entry = new OrderHistoryEntry { Date = DateTimeOffset.Now, Event = "Created" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new[] { entry })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var orders = new OrdersClient(client);

        var result = await orders.GetHistoryAsync(7);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/order/7/history", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Created", result[0].Event);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task GetHistoryAsync_InvalidOrderId_Throws(int orderId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var orders = new OrdersClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => orders.GetHistoryAsync(orderId));
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
    public async Task EnumerateOrdersAsync_ReturnsPages() {
        var page1 = new[] { new Order { Id = 1 }, new Order { Id = 2 } };
        var page2 = new[] { new Order { Id = 3 } };

        var responses = new[] {
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(page1) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(page2) }
        };

        var handler = new SequenceHandler(responses);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var orders = new OrdersClient(client);

        var results = new List<Order>();
        await foreach (var order in orders.EnumerateOrdersAsync(pageSize: 2)) {
            results.Add(order);
        }

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("https://example.com/v1/order?size=2&position=0", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal("https://example.com/v1/order?size=2&position=2", handler.Requests[1].RequestUri!.ToString());
        Assert.Equal(3, results.Count);
        Assert.Equal(3, results[2].Id);
    }

    private sealed class TestProgress : IProgress<double> {
        public double Value { get; private set; }
        public void Report(double value) => Value = value;
    }

    [Fact]
    public async Task UploadAsync_SendsFile() {
        using var stream = new MemoryStream(new byte[10]);
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var orders = new OrdersClient(client);

        await orders.UploadAsync(stream, "text/csv");

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://example.com/v1/order/bulk", handler.Request.RequestUri!.ToString());
        Assert.Equal("text/csv", handler.Request.Content!.Headers.ContentType!.MediaType);
    }

    [Fact]
    public async Task UploadAsync_ReportsProgress() {
        using var stream = new MemoryStream(new byte[20]);
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var orders = new OrdersClient(client);
        var progress = new TestProgress();

        await orders.UploadAsync(stream, "application/json", progress);

        Assert.True(progress.Value > 0);
    }
}