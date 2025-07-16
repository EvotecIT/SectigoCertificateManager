using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Models;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="InventoryClient"/>.
/// </summary>
public sealed class InventoryClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            return Task.FromResult(_response);
        }
    }

    [Fact]
    public async Task DownloadCsvAsync_BuildsQueryAndParsesCsv() {
        const string csv = "id,commonName\n1,example.com";
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(csv) };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var inventory = new InventoryClient(client);

        var request = new InventoryCsvRequest { Size = 5, Position = 10 };
        var result = await inventory.DownloadCsvAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/inventory.csv?size=5&position=10", handler.Request!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("example.com", result[0].CommonName);
    }

    [Fact]
    public async Task DownloadCsvAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var inventory = new InventoryClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => inventory.DownloadCsvAsync(null!));
    }

    [Fact]
    public async Task DownloadCsvAsync_DisposesResponse() {
        const string csv = "id,commonName\n1,example.com";
        var response = new DisposableResponse { Content = new StringContent(csv), StatusCode = HttpStatusCode.OK };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var inventory = new InventoryClient(client);

        _ = await inventory.DownloadCsvAsync(new InventoryCsvRequest());

        Assert.True(response.Disposed);
    }
}
