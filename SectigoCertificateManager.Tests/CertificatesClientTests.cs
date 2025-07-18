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
using System.Text;
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

    private sealed class TestProgress : IProgress<double> {
        public double Value { get; private set; }
        public void Report(double value) => Value = value;
    }

    private const string Base64Cert = "MIIC/zCCAeegAwIBAgIULTQw6ATwfRI/1hVSQooJNHPEit8wDQYJKoZIhvcNAQELBQAwDzENMAsGA1UEAwwEdGVzdDAeFw0yNTA3MDQxMzE2NDRaFw0yNTA3MDUxMzE2NDRaMA8xDTALBgNVBAMMBHRlc3QwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQDiO8kIwsJLCi3d8bX31IIISKSoA24iCcfV7m+uMm8CMdJlY2NGf8ThiF3suG2lHQCxESQacUrPFMN/J3cM7L+5R8p24CCnrmAP2WhMuO2IwFhgfjo4PsmnmCGNx5fDAPI+lnSS6pnHfZfAPw3dbPT2/cgbeil0q2ByFR6C2YXU+mFdOg7cJJ1f2GXbUL3QYRBuaDYCHRrDAym4e/8DkKjjaroDxw1BPD6sjvzrDdEDusJANDCp8K6Cr99nvG+YCLjueN+xvUXHbsp9gUfLI39X73p+M9zGcYGAeYyD/i+VM/+Kde5CEfS34eOKfRIJX6DHAbVu1SrJPNFFvQV0keb/AgMBAAGjUzBRMB0GA1UdDgQWBBQ8PwJEkQsHvU7i5i45XLLyJUi4eTAfBgNVHSMEGDAWgBQ8PwJEkQsHvU7i5i45XLLyJUi4eTAPBgNVHRMBAf8EBTADAQH/MA0GCSqGSIb3DQEBCwUAA4IBAQAjWADB2IC5xBHKOROcXZDa8mp3DaasUwL5mWjG7Ppr4LHrY1uCEojstJCg6s2FLBjGTs+0DTQ5UiBqSVJDK1GVhYG02xJSPoXNS4wNTp4a56NtbkDT96lO0BrH91lclMNXHU9NpMUFea0tt7h5tUeVtZ2CVK0nuy5MOifMdURVyhWsFgQVemmTNTYisVD5sNRvBJEq0M+3+JSjFYvRZVqfRSM3z1K4XcZJfhxv7Gq1ebb93R1QunIdGC0HiFnBZxpxhDCbcVOpbdbQOJ22dLSe5/4f+1V+D/bPCZJx5kF0yvM0jEhuQNxNV3H/DasvBhH/24JIjpe+WfKPw0jx7vR6";

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
            IssuerDn = "CN=te st",
            KeyAlgorithm = "RSA/DSA",
            DateFrom = new DateTime(2023, 7, 1),
            DateTo = new DateTime(2023, 7, 31)
        };
        await certificates.SearchAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate?size=10&position=5&commonName=te%20st&status=Issued&sslTypeId=2&issuer=A%26B&issuerDN=CN%3Dte%20st&keyAlgorithm=RSA%2FDSA&dateFrom=2023-07-01&dateTo=2023-07-31", handler.Request!.RequestUri!.AbsoluteUri);
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

    [Fact]
    public async Task DownloadAsync_ReportsProgress() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("0123456789")
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);
        var progress = new TestProgress();

        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            await certificates.DownloadAsync(2, path, progress: progress);
            Assert.Equal(1d, progress.Value, 3);
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
    public async Task GetRevocationAsync_ReturnsDetails() {
        var revocation = new CertificateRevocation { Reason = "compromised" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(revocation)
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        var result = await certificates.GetRevocationAsync(8);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate/8/revocation", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal("compromised", result!.Reason);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    public async Task GetRevocationAsync_InvalidCertificateId_Throws(int certificateId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => certificates.GetRevocationAsync(certificateId));
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
    public async Task EnumerateCertificatesAsync_ReturnsPages() {
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
        await foreach (var certificate in certificates.EnumerateCertificatesAsync(pageSize: 2)) {
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

    [Fact]
    public async Task ImportAsync_SendsMultipartRequest() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new ImportCertificateResponse { ProcessedCount = 2, Errors = new[] { "err" } })
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("DATA"));
        var result = await certificates.ImportAsync(10, stream, "certs.zip");

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://example.com/v1/certificate/import?orgId=10", handler.Request.RequestUri!.ToString());
        Assert.NotNull(handler.Request.Content);
        Assert.StartsWith("multipart/form-data", handler.Request.Content!.Headers.ContentType!.MediaType);
        Assert.NotNull(result);
        Assert.Equal(2, result!.ProcessedCount);
        Assert.Single(result.Errors);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task ImportAsync_InvalidOrgId_Throws(int orgId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        using var stream = new MemoryStream();
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => certificates.ImportAsync(orgId, stream, "certs.zip"));
    }

    [Fact]
    public async Task ValidateCertificateRequestAsync_SendsRequest() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new ValidateCertificateResponse { IsValid = true })
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        var request = new ValidateCertificateRequest { Csr = "csrdata" };
        var result = await certificates.ValidateCertificateRequestAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/certificate/validate", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"csr\":\"csrdata\"", handler.Body);
        Assert.NotNull(result);
        Assert.True(result!.IsValid);
    }

    [Fact]
    public async Task ValidateCertificateRequestAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(new ValidateCertificateResponse()) });
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => certificates.ValidateCertificateRequestAsync(null!));
    }

    [Fact]
    public async Task DownloadX509Async_ReturnsCertificate() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Base64Cert) };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        using var cert = await certificates.DownloadX509Async(5);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/ssl/v1/collect/5?format=base64", handler.Request!.RequestUri!.ToString());
        Assert.Equal("51A908D14C9C984231B7E2F6C37ABB1368A57F1F", cert.Thumbprint);
    }

    [Fact]
    public async Task DownloadX509Async_InvalidData_Throws() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("???") };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var certificates = new CertificatesClient(client);

        await Assert.ThrowsAsync<ValidationException>(() => certificates.DownloadX509Async(5));
    }
}