using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="AuditClient"/>.
/// </summary>
public sealed class AuditClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
#if NETSTANDARD2_0 || NET472
            if (request.Content is not null) {
                await request.Content.CopyToAsync(Stream.Null).ConfigureAwait(false);
            }
#else
            if (request.Content is not null) {
                await request.Content.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);
            }
#endif
            return _response;
        }
    }

    [Fact]
    public async Task ListAsync_RequestsLogs() {
        var entry = new AuditLogEntry { Id = 1, Guid = "g" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new { reports = new[] { entry } })
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_6), httpClient);
        var audits = new AuditClient(client);

        var result = await audits.ListAsync();

        Assert.NotNull(handler.Request);
        var uri = handler.Request!.RequestUri!;
        Assert.Equal("https://example.com/report/v1/activity", uri.GetLeftPart(UriPartial.Path));
        Assert.Contains("size=200", uri.Query);
        Assert.Contains("position=0", uri.Query);
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    private sealed class SequenceHandler : HttpMessageHandler {
        private readonly Queue<HttpResponseMessage> _responses;
        public List<HttpRequestMessage> Requests { get; } = new();

        public SequenceHandler(IEnumerable<HttpResponseMessage> responses) {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Requests.Add(request);
#if NETSTANDARD2_0 || NET472
            if (request.Content is not null) {
                await request.Content.CopyToAsync(Stream.Null).ConfigureAwait(false);
            }
#else
            if (request.Content is not null) {
                await request.Content.CopyToAsync(Stream.Null, cancellationToken).ConfigureAwait(false);
            }
#endif
            return _responses.Dequeue();
        }
    }

    [Fact]
    public async Task EnumerateAsync_ReturnsPages() {
        var page1 = new[] { new AuditLogEntry { Id = 1 } };
        var page2 = new[] { new AuditLogEntry { Id = 2 } };
        var empty = Array.Empty<AuditLogEntry>();

        var responses = new[] {
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { reports = page1 }) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { reports = page2 }) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new { reports = empty }) }
        };

        var handler = new SequenceHandler(responses);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_6), httpClient);
        var audits = new AuditClient(client);

        var results = new List<AuditLogEntry>();
        await foreach (var e in audits.EnumerateAsync(pageSize: 1)) {
            results.Add(e);
        }

        Assert.Equal(3, handler.Requests.Count);
        AssertAll(handler.Requests[0], 0);
        AssertAll(handler.Requests[1], 1);
        AssertAll(handler.Requests[2], 2);
        Assert.Equal(2, results.Count);
    }

    private static void AssertAll(HttpRequestMessage request, int position) {
        var uri = request.RequestUri!;
        Assert.Equal("https://example.com/report/v1/activity", uri.GetLeftPart(UriPartial.Path));
        Assert.Contains("size=1", uri.Query);
        Assert.Contains($"position={position}", uri.Query);
    }
}
