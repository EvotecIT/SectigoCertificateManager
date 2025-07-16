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
using System.Collections.Generic;
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
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
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
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
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
    public void IssueCertificateRequest_InvalidTerm_Throws(int term) {
        var request = new IssueCertificateRequest();

        Assert.Throws<ArgumentOutOfRangeException>(() => request.Term = term);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task GetAsync_InvalidCertificateId_Throws(int certificateId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => certificates.GetAsync(certificateId));
    }

    [Fact]
    public async Task RevokeAsync_SendsPayload() {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
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
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
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
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
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
    public async Task RenewByOrderNumberAsync_SendsRequestAndReturnsId() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new RenewCertificateResponse { SslId = 11 })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new RenewCertificateRequest { Csr = "csr", DcvMode = "EMAIL", DcvEmail = "admin@example.com" };
        var result = await certificates.RenewByOrderNumberAsync(555, request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate/renew/555", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"csr\":\"csr\"", handler.Body);
        Assert.Contains("\"dcvMode\":\"EMAIL\"", handler.Body);
        Assert.Contains("\"dcvEmail\":\"admin@example.com\"", handler.Body);
        Assert.Equal(11, result);
    }

    [Fact]
    public async Task SearchAsync_EncodesAndOrdersQueryParameters() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(Array.Empty<Certificate>())
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
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
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => certificates.RevokeAsync(null!));
    }

    [Fact]
    public async Task RenewAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new RenewCertificateResponse { SslId = 1 }) });
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => certificates.RenewAsync(1, null!));
    }

    [Theory]
    [InlineData(2, true)]
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public async Task DownloadAsync_WritesFile(int certificateId, bool isValid) {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("DATA")
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(dir, Path.GetRandomFileName());
        try {
            if (isValid) {
                await certificates.DownloadAsync(certificateId, path);
                Assert.NotNull(handler.Request);
                Assert.Equal($"https://example.com/ssl/v1/collect/{certificateId}?format=base64", handler.Request!.RequestUri!.ToString());
                Assert.True(Directory.Exists(dir));
                Assert.True(File.Exists(path));
                Assert.Equal("DATA", File.ReadAllText(path));
            } else {
                await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => certificates.DownloadAsync(certificateId, path));
                Assert.False(Directory.Exists(dir));
            }
        } finally {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public async Task DownloadAsync_InvalidPath_Throws(string? path) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentException>(() => certificates.DownloadAsync(1, path!));
    }

    [Fact]
    public async Task DownloadAsync_WithBaseUrlPath_UsesCorrectUri() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("DATA")
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/api", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            await certificates.DownloadAsync(2, path);
            Assert.NotNull(handler.Request);
            Assert.Equal("https://example.com/api/ssl/v1/collect/2?format=base64", handler.Request!.RequestUri!.ToString());
        } finally {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [MemberData(nameof(StatusCases))]
    public async Task GetStatusAsync_ReturnsStatus(string text, CertificateStatus expected) {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new { Status = text })
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        var result = await certificates.GetStatusAsync(3);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate/3/status", handler.Request!.RequestUri!.ToString());
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> StatusCases() {
        foreach (CertificateStatus status in Enum.GetValues(typeof(CertificateStatus))) {
            yield return new object[] { status.ToString(), status };
        }
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetStatusAsync_InvalidCertificateId_Throws(int certificateId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => certificates.GetStatusAsync(certificateId));
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteRequest() {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        await certificates.DeleteAsync(6);

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Delete, handler.Request!.Method);
        Assert.Equal("https://example.com/v1/certificate/6", handler.Request.RequestUri!.ToString());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-2)]
    public async Task DeleteAsync_InvalidCertificateId_Throws(int certificateId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.NoContent));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => certificates.DeleteAsync(certificateId));
    }

    [Fact]
    public async Task SearchAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(Array.Empty<Certificate>()) });
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => certificates.SearchAsync(null!));
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
    public async Task EnumerateSearchAsync_ReturnsPages() {
        var page1 = new[] { new Certificate { Id = 1 }, new Certificate { Id = 2 } };
        var page2 = new[] { new Certificate { Id = 3 } };

        var responses = new[] {
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(page1) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(page2) }
        };

        var handler = new SequenceHandler(responses);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var results = new List<Certificate>();
        await foreach (var certificate in certificates.EnumerateSearchAsync(new CertificateSearchRequest { Size = 2 })) {
            results.Add(certificate);
        }

        Assert.Equal(2, handler.Requests.Count);
        Assert.Equal("https://example.com/v1/certificate?size=2", handler.Requests[0].RequestUri!.ToString());
        Assert.Equal("https://example.com/v1/certificate?size=2&position=2", handler.Requests[1].RequestUri!.ToString());
        Assert.Equal(3, results.Count);
        Assert.Equal(3, results[2].Id);
    }

    [Fact]
    public async Task SearchAsync_MultiplePages_ReturnsAllResults() {
        var page1 = new[] { new Certificate { Id = 1 }, new Certificate { Id = 2 } };
        var page2 = new[] { new Certificate { Id = 3 } };

        var responses = new[] {
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(page1) },
            new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(page2) }
        };

        var handler = new SequenceHandler(responses);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var certificates = new CertificatesClient(client);

        var request = new CertificateSearchRequest { Size = 2 };
        var result = await certificates.SearchAsync(request);

        Assert.NotNull(result);
        Assert.Equal(3, result!.Certificates.Count);
    }
}