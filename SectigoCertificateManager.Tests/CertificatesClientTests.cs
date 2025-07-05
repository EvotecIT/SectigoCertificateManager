using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class CertificatesClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            if (request.Content is not null) {
                Body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            return _response;
        }
    }

    [Fact]
    public async Task SearchAsync_BuildsQueryAndParsesResponse() {
        var certificate = new Certificate { Id = 1, CommonName = "test" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
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

    [Fact]
    public async Task IssueAsync_ReturnsCertificate() {
        var certificate = new Certificate { Id = 2, CommonName = "example.com" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(certificate)
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new IssueCertificateRequest { CommonName = "example.com", ProfileId = 1, Term = 12 };
        var result = await certificates.IssueAsync(request);

        Assert.NotNull(handler.Request);
        var actualRequest = handler.Request!;
        Assert.Equal("https://example.com/v1/certificate/issue", actualRequest.RequestUri!.ToString());
        Assert.NotNull(result);
        var actualResult = result!;
        Assert.Equal(2, actualResult.Id);
        Assert.Equal("example.com", actualResult.CommonName);
    }

    [Fact]
    public async Task RevokeAsync_SendsPayload() {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new RevokeCertificateRequest { CertId = 5, ReasonCode = 4, Reason = "superseded" };
        await certificates.RevokeAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate/revoke", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"certId\":5", handler.Body);
        Assert.Contains("\"reasonCode\":4", handler.Body);
        Assert.Contains("\"reason\":\"superseded\"", handler.Body);
    }
}