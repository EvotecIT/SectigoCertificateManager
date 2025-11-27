using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="CertificateService"/>.
/// </summary>
public sealed class CertificateServiceTests {
    private sealed class AdminHandler : HttpMessageHandler {
        private readonly string _tokenJson;
        private readonly string _apiJson;
        private readonly HttpResponseMessage? _streamResponse;

        public HttpRequestMessage? LastRequest { get; private set; }

        public AdminHandler(HttpResponseMessage tokenResponse, HttpResponseMessage apiResponse, HttpResponseMessage? streamResponse = null) {
            _tokenJson = tokenResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _apiJson = apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _streamResponse = streamResponse;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            LastRequest = request;
            if (request.RequestUri!.AbsoluteUri.Contains("protocol/openid-connect/token")) {
                var response = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(_tokenJson, System.Text.Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            }

            if (_streamResponse is not null && request.RequestUri!.AbsoluteUri.Contains("/collect/")) {
                return Task.FromResult(_streamResponse);
            }

            var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(_apiJson, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(apiResponse);
        }
    }

    private sealed class AdminListThenDetailHandler : HttpMessageHandler {
        private readonly string _tokenJson;
        private readonly string _listJson;
        private readonly string _detailJson;

        public HttpRequestMessage? LastRequest { get; private set; }

        public AdminListThenDetailHandler(
            HttpResponseMessage tokenResponse,
            HttpResponseMessage listResponse,
            HttpResponseMessage detailResponse) {
            _tokenJson = tokenResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _listJson = listResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _detailJson = detailResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            LastRequest = request;
            var uri = request.RequestUri!.AbsoluteUri;

            if (uri.Contains("protocol/openid-connect/token")) {
                var response = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(_tokenJson, Encoding.UTF8, "application/json")
                };
                return Task.FromResult(response);
            }

            if (uri.Contains("/api/ssl/v2?")) {
                var listResponse = new HttpResponseMessage(HttpStatusCode.OK) {
                    Content = new StringContent(_listJson, Encoding.UTF8, "application/json")
                };
                return Task.FromResult(listResponse);
            }

            var detailResponse = new HttpResponseMessage(HttpStatusCode.OK) {
                Content = new StringContent(_detailJson, Encoding.UTF8, "application/json")
            };
            return Task.FromResult(detailResponse);
        }
    }

    private sealed class LegacyStubClient : ISectigoClient {
        private readonly HttpResponseMessage _response;

        public HttpRequestMessage? LastRequest { get; private set; }

        public HttpClient HttpClient { get; } = new();

        public LegacyStubClient(HttpResponseMessage response) {
            _response = response;
        }

        public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default) {
            LastRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return Task.FromResult(_response);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }

        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default) {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public async Task ListAsync_Admin_ReturnsMappedCertificates() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var identities = new[] {
            new AdminSslIdentity { SslId = 1, CommonName = "example.com", SerialNumber = "123" }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(identities)
        };

        var handler = new AdminHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var adminConfig = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");

        using var service = new CertificateService(adminConfig, http);
        var result = await service.ListAsync(10, 0);

        Assert.Single(result);
        var cert = result[0];
        Assert.Equal(1, cert.Id);
        Assert.Equal("example.com", cert.CommonName);
        Assert.Equal("123", cert.SerialNumber);
    }

    [Fact]
    public async Task ListDetailedAsync_Admin_UsesDetailsEndpoint() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var identities = new[] {
            new AdminSslIdentity { SslId = 1, CommonName = "summary", SerialNumber = "SUMMARY" }
        };
        var details = new AdminSslCertificateDetails {
            Id = 1,
            CommonName = "detailed.example.com",
            OrgId = 10,
            Status = "Issued",
            SerialNumber = "DETAIL",
            Term = 365
        };

        var listResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create<IReadOnlyList<AdminSslIdentity>>(identities)
        };
        var detailResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(details)
        };

        var handler = new AdminListThenDetailHandler(tokenResponse, listResponse, detailResponse);

        using var http = new HttpClient(handler);
        var adminConfig = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");

        using var service = new CertificateService(adminConfig, http);

        var certificates = await service.ListDetailedAsync(10, 0);

        Assert.Single(certificates);
        var cert = certificates[0];
        Assert.Equal(1, cert.Id);
        Assert.Equal("detailed.example.com", cert.CommonName);
        Assert.Equal("DETAIL", cert.SerialNumber);
        Assert.Equal(10, cert.OrgId);
        Assert.Equal(365, cert.Term);
        Assert.Equal(CertificateStatus.Issued, cert.Status);
    }

    [Fact]
    public async Task GetAsync_Admin_ReturnsMappedCertificate() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var details = new AdminSslCertificateDetails {
            Id = 5,
            CommonName = "example.org",
            OrgId = 7,
            Status = "Issued",
            SerialNumber = "ABC",
            ReasonCode = "4",
            Revoked = "2025-01-02T03:04:05Z"
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(details)
        };

        var handler = new AdminHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var adminConfig = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");

        using var service = new CertificateService(adminConfig, http);
        var cert = await service.GetAsync(5);
        var status = await service.GetStatusAsync(5);
        var revocation = await service.GetRevocationAsync(5);

        Assert.NotNull(cert);
        Assert.Equal(5, cert!.Id);
        Assert.Equal("example.org", cert.CommonName);
        Assert.Equal(7, cert.OrgId);
        Assert.Equal("ABC", cert.SerialNumber);
        Assert.Equal(CertificateStatus.Issued, cert.Status);

        Assert.Equal(CertificateStatus.Issued, status);
        Assert.NotNull(revocation);
        Assert.Equal(5, revocation!.CertId);
        Assert.Equal("ABC", revocation.SerialNumber);
        Assert.Equal(RevocationReason.Superseded, revocation.ReasonCode);
    }

    [Fact]
    public async Task ListAsync_Legacy_UsesCertificatesClient() {
        var certificates = new[] {
            new Certificate { Id = 1, CommonName = "legacy.example.com" }
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create<IReadOnlyList<Certificate>>(certificates)
        };

        var stub = new LegacyStubClient(response);
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);

        using var service = new CertificateService(config, stub);
        var result = await service.ListAsync(10, 0);

        Assert.NotNull(stub.LastRequest);
        Assert.Equal("v1/certificate?size=10&position=0", stub.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("legacy.example.com", result[0].CommonName);
    }

    [Fact]
    public async Task GetAsync_Legacy_UsesCertificatesClient() {
        var certificate = new Certificate {
            Id = 2,
            CommonName = "legacy.example.org"
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(certificate)
        };

        var stub = new LegacyStubClient(response);
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);

        using var service = new CertificateService(config, stub);
        var result = await service.GetAsync(2);

        Assert.NotNull(stub.LastRequest);
        Assert.Equal("v1/certificate/2", stub.LastRequest!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
        Assert.Equal("legacy.example.org", result.CommonName);
    }

    [Fact]
    public async Task EnumerateAsync_Admin_StreamsCertificates() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var identities = new[] {
            new AdminSslIdentity { SslId = 1, CommonName = "example.com", SerialNumber = "123" },
            new AdminSslIdentity { SslId = 2, CommonName = "example.org", SerialNumber = "456" }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create<IReadOnlyList<AdminSslIdentity>>(identities)
        };

        var handler = new AdminHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var adminConfig = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");

        using var service = new CertificateService(adminConfig, http);
        var results = new List<Certificate>();
        await foreach (var cert in service.EnumerateAsync(pageSize: 10)) {
            results.Add(cert);
        }

        Assert.Equal(2, results.Count);
        Assert.Equal(1, results[0].Id);
        Assert.Equal(2, results[1].Id);
        Assert.Equal("example.com", results[0].CommonName);
        Assert.Equal("example.org", results[1].CommonName);
    }
}
