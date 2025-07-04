using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class CertificatesClientTests
{
    private sealed class StubClient : ISectigoClient
    {
        public string? RequestUri { get; private set; }
        public HttpMethod? Method { get; private set; }

        public HttpClient HttpClient => throw new NotImplementedException();

        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
        {
            RequestUri = requestUri;
            Method = HttpMethod.Delete;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteRequest()
    {
        var client = new StubClient();
        var certificates = new CertificatesClient(client);

        await certificates.DeleteAsync(123);

        Assert.Equal("v1/certificate/123", client.RequestUri);
        Assert.Equal(HttpMethod.Delete, client.Method);
    }
}
