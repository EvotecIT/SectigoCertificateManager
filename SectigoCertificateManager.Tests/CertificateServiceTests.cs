using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Models;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
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

}
