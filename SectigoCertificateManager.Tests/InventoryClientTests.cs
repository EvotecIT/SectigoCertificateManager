using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Models;
using System.Net;
using System.Net.Http;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="InventoryClient"/>.
/// </summary>
public sealed class InventoryClientTests {
    private sealed class StubClient : ISectigoClient {
        private readonly HttpResponseMessage _response;
        public HttpClient HttpClient { get; } = new();

        public StubClient(HttpResponseMessage response) {
            _response = response;
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
            => Task.FromResult(_response);

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    private sealed class TrackingContent : StringContent {
        public bool Disposed { get; private set; }

        public TrackingContent(string content) : base(content) {
        }

        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
            Disposed = true;
        }
    }

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
    public async Task DownloadCsvAsync_DisposesResponseAfterParsing() {
        const string csv = "id,commonName\n1,example.com";
        var content = new TrackingContent(csv);
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = content };
        var inventory = new InventoryClient(new StubClient(response));

        var result = await inventory.DownloadCsvAsync(new InventoryCsvRequest { Size = 1 });

        Assert.Single(result);
        Assert.True(content.Disposed);
    }

    [Fact]
    public async Task DownloadCsvAsync_UsesInvariantCulture_DefaultCulture() {
        const string csv = "id,commonName\n1,example.com";
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(csv) };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var inventory = new InventoryClient(client);

        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try {
            CultureInfo.CurrentCulture = new CultureInfo("fr-FR");
            CultureInfo.CurrentUICulture = new CultureInfo("fr-FR");

            var request = new InventoryCsvRequest {
                DateFrom = new DateTime(2023, 1, 1),
                DateTo = new DateTime(2023, 1, 31)
            };
            await inventory.DownloadCsvAsync(request);

            Assert.NotNull(handler.Request);
            Assert.Equal("https://example.com/v1/inventory.csv?from=2023-01-01&to=2023-01-31", handler.Request!.RequestUri!.ToString());
        } finally {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }

    [Fact]
    public async Task DownloadCsvAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var inventory = new InventoryClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => inventory.DownloadCsvAsync(null!));
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("de-DE")]
    public async Task DownloadCsvAsync_UsesInvariantCulture(string cultureName) {
        var originalCulture = CultureInfo.CurrentCulture;
        var originalUiCulture = CultureInfo.CurrentUICulture;
        try {
            var culture = new CultureInfo(cultureName);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;

            const string csv = "id,commonName\n1,example.com";
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(csv) };

            var handler = new TestHandler(response);
            var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
            var inventory = new InventoryClient(client);

            var request = new InventoryCsvRequest { DateFrom = new DateTime(2023, 5, 1), DateTo = new DateTime(2023, 5, 7) };
            await inventory.DownloadCsvAsync(request);

            Assert.NotNull(handler.Request);
            Assert.Equal("https://example.com/v1/inventory.csv?from=2023-05-01&to=2023-05-07", handler.Request!.RequestUri!.ToString());
        } finally {
            CultureInfo.CurrentCulture = originalCulture;
            CultureInfo.CurrentUICulture = originalUiCulture;
        }
    }
}
