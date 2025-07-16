using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="CertificatesClient"/>.
/// </summary>
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

    /// <summary>Parses search results.</summary>
    [Fact]
    public async Task SearchAsync_BuildsQueryAndParsesResponse() {
        var certificate = new Certificate { Id = 1, CommonName = "test" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new[] { certificate })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new CertificateSearchRequest {
            Size = 5,
            CommonName = "test",
            Status = CertificateStatus.Issued,
            DateFrom = new DateTime(2023, 1, 1),
            DateTo = new DateTime(2023, 1, 31)
        };
        var result = await certificates.SearchAsync(request);

        Assert.NotNull(handler.Request);
        var actualRequest = handler.Request!;
        Assert.Equal("https://example.com/v1/certificate?size=5&commonName=test&status=Issued&dateFrom=2023-01-01&dateTo=2023-01-31", actualRequest.RequestUri!.ToString());
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

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task IssueAsync_InvalidTerm_Throws(int term) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new IssueCertificateRequest { CommonName = "example.com", ProfileId = 1, Term = term };
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => certificates.IssueAsync(request));
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

    [Fact]
    public async Task SearchAsync_EmptyRequest_UsesBaseEndpoint() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(Array.Empty<Certificate>())
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new CertificateSearchRequest();
        await certificates.SearchAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate", handler.Request!.RequestUri!.ToString());
    }

    [Fact]
    public async Task RenewAsync_SendsRequestAndReturnsId() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new RenewCertificateResponse { SslId = 10 })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new RenewCertificateRequest { Csr = "csr", DcvMode = "EMAIL", DcvEmail = "admin@example.com" };
        var result = await certificates.RenewAsync(7, request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate/renewById/7", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"csr\":\"csr\"", handler.Body);
        Assert.Contains("\"dcvMode\":\"EMAIL\"", handler.Body);
        Assert.Contains("\"dcvEmail\":\"admin@example.com\"", handler.Body);
        Assert.Equal(10, result);
    }

    [Fact]
    public async Task SearchAsync_EncodesAndOrdersQueryParameters() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(Array.Empty<Certificate>())
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new CertificateSearchRequest {
            Size = 10,
            Position = 5,
            CommonName = "te st",
            Status = CertificateStatus.Issued,
            SslTypeId = 2,
            Issuer = "A&B",
            KeyAlgorithm = "RSA/DSA",
            DateFrom = new DateTime(2023, 7, 1),
            DateTo = new DateTime(2023, 7, 31)
        };
        await certificates.SearchAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate?size=10&position=5&commonName=te%20st&status=Issued&sslTypeId=2&issuer=A%26B&keyAlgorithm=RSA%2FDSA&dateFrom=2023-07-01&dateTo=2023-07-31", handler.Request!.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task RevokeAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => certificates.RevokeAsync(null!));
    }

    [Fact]
    public async Task RenewAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new RenewCertificateResponse { SslId = 1 }) });
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => certificates.RenewAsync(1, null!));
    }

    [Fact]
    public async Task DownloadAsync_WritesFile() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("DATA")
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            await certificates.DownloadAsync(2, path);
            Assert.NotNull(handler.Request);
            Assert.Equal("https://example.com/ssl/v1/collect/2?format=base64", handler.Request!.RequestUri!.ToString());
            Assert.True(File.Exists(path));
            Assert.Equal("DATA", File.ReadAllText(path));
        } finally {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task DownloadAsync_InvalidPath_Throws(string? path) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentException>(() => certificates.DownloadAsync(1, path!));
    }

    [Fact]
    public async Task SearchAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<Certificate>()) });
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => certificates.SearchAsync(null!));
    }
}