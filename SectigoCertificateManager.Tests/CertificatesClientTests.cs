using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System;
using System.Net;
using System.Net.Http;
using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.Net.Http.Json;
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
    
    [Fact]
    private sealed class TestHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(_response);
        }
    }

    [Fact]
    public async Task SearchAsync_BuildsQueryAndParsesResponse()
    {
        var certificate = new Certificate { Id = 1, CommonName = "test" };
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new[] { certificate })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new CertificateSearchRequest { Size = 5, CommonName = "test", Status = CertificateStatus.Issued };
        var result = await certificates.SearchAsync(request);

        Assert.NotNull(handler.Request);
        var actualRequest = handler.Request!;
        Assert.Equal("https://example.com/v1/certificate?size=5&commonName=test&status=Issued", actualRequest.RequestUri!.ToString());
        Assert.NotNull(result);
        var actualResult = result!;
        Assert.Single(actualResult.Certificates);
        Assert.Equal(1, actualResult.Certificates[0].Id);
    }
}
