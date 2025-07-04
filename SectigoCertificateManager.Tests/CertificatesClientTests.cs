using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Responses;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class CertificatesClientTests
{
    private sealed class PagingHandler : HttpMessageHandler
    {
        private readonly Queue<HttpResponseMessage> _responses;
        public List<HttpRequestMessage> Requests { get; } = new();

        public PagingHandler(IEnumerable<HttpResponseMessage> responses)
        {
            _responses = new Queue<HttpResponseMessage>(responses);
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Requests.Add(request);
            return Task.FromResult(_responses.Dequeue());
        }
    }

    [Fact]
    public async Task ListAsync_ReturnsItemsFromAllPages()
    {
        var first = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CertificateResponse
            {
                Certificates = new[] { new Certificate { Id = 1 }, new Certificate { Id = 2 } }
            })
        };

        var second = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new CertificateResponse
            {
                Certificates = new[] { new Certificate { Id = 3 } }
            })
        };

        var handler = new PagingHandler(new[] { first, second });
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);
        var client = new SectigoClient(config, new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var ids = new List<int>();
        await foreach (var cert in certificates.ListAsync(pageSize: 2))
        {
            ids.Add(cert.Id);
        }

        Assert.Equal(new[] { 1, 2, 3 }, ids);
        Assert.Collection(handler.Requests,
            r => Assert.Equal("https://example.com/v1/certificate?page=1&size=2", r.RequestUri!.ToString()),
            r => Assert.Equal("https://example.com/v1/certificate?page=2&size=2", r.RequestUri!.ToString()));
    }
}
