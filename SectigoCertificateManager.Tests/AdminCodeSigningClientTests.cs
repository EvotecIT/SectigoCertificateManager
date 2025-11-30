using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public class AdminCodeSigningClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _tokenResponse;
        private readonly HttpResponseMessage _apiResponse;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }

        public TestHandler(HttpResponseMessage tokenResponse, HttpResponseMessage apiResponse) {
            _tokenResponse = tokenResponse;
            _apiResponse = apiResponse;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (request.RequestUri is not null &&
                request.RequestUri.AbsoluteUri.IndexOf("/protocol/openid-connect/token", StringComparison.OrdinalIgnoreCase) >= 0) {
                return _tokenResponse;
            }

            LastRequest = request;
            if (request.Content is not null) {
                LastRequestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return _apiResponse;
        }
    }

    [Fact]
    public async Task ImportAsync_PostsArrayToImportEndpoint() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var results = new[] {
            new CertificateImportResult {
                Successful = true,
                BackendCertId = "cs-1",
                Cert = null,
                Created = true,
                ErrorMessage = null
            }
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(results)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);

        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");

        var client = new AdminCodeSigningClient(config, http);

        var requests = new[] {
            new CertificateImportRequest { OrgId = 1 }
        };

        var response = await client.ImportAsync(requests, CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/cscert/v1/import", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);

        using var bodyDoc = JsonDocument.Parse(handler.LastRequestBody!);
        Assert.Equal(JsonValueKind.Array, bodyDoc.RootElement.ValueKind);

        Assert.Single(response);
        Assert.True(response[0].Successful);
        Assert.Equal("cs-1", response[0].BackendCertId);
    }

    [Fact]
    public async Task RevokeManualAsync_BuildsUriAndBody() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);

        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");

        var client = new AdminCodeSigningClient(config, http);

        var revokeDate = System.DateTimeOffset.UtcNow;
        await client.RevokeManualAsync(certId: 10, serialNumber: "ABC", issuer: "CN=Issuer", revokeDate: revokeDate, reasonCode: "4", reason: "Testing");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/cscert/v1/revoke/manual", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);

        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal(10, body.GetProperty("certId").GetInt32());
        Assert.Equal("ABC", body.GetProperty("serialNumber").GetString());
        Assert.Equal("CN=Issuer", body.GetProperty("issuer").GetString());
        Assert.Equal("4", body.GetProperty("reasonCode").GetString());
        Assert.Equal("Testing", body.GetProperty("reason").GetString());
    }
}
