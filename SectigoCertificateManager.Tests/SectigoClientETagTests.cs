using SectigoCertificateManager;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests ETag caching behavior of <see cref="SectigoClient"/>.
/// </summary>
public sealed class SectigoClientETagTests {
    private sealed class SequenceHandler : HttpMessageHandler {
        private readonly Queue<HttpResponseMessage> _responses;
        public List<HttpRequestMessage> Requests { get; } = new();

        public SequenceHandler(IEnumerable<HttpResponseMessage> responses) {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Requests.Add(request);
            return Task.FromResult(_responses.Dequeue());
        }
    }

    /// <summary>Stores ETag and sends If-None-Match for subsequent requests.</summary>
    [Fact]
    public async Task SendsIfNoneMatchAndHandlesNotModified() {
        var responses = new[] {
            new HttpResponseMessage(HttpStatusCode.OK) { Headers = { ETag = new EntityTagHeaderValue("\"v1\"") } },
            new HttpResponseMessage(HttpStatusCode.NotModified)
        };

        var handler = new SequenceHandler(responses);
        using var httpClient = new HttpClient(handler);
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://example.com/")
            .WithCredentials("u", "p")
            .WithCustomerUri("c")
            .WithETagCache()
            .Build();

        var client = new SectigoClient(config, httpClient);
        var first = await client.GetAsync("v1/test");
        var second = await client.GetAsync("v1/test");

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.NotModified, second.StatusCode);
        var header = Assert.Single(handler.Requests[1].Headers.IfNoneMatch);
        Assert.Equal("\"v1\"", header.Tag);
    }
}
