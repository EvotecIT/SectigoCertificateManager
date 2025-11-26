using SectigoCertificateManager.AdminApi;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="AdminSslClient"/>.
/// </summary>
public sealed class AdminSslClientTests {
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
            LastRequest = request;

            if (request.Content is not null) {
                LastRequestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            } else {
                LastRequestBody = null;
            }

            if (request.RequestUri!.AbsoluteUri.Contains("protocol/openid-connect/token")) {
                return _tokenResponse;
            }

            return _apiResponse;
        }
    }

    [Fact]
    public async Task EnrollAsync_PostsToEnrollEndpoint() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var enrollResponse = new AdminSslEnrollResponse {
            SslId = 10,
            RenewId = "r1"
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(enrollResponse)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var request = new AdminSslEnrollRequest {
            OrgId = 5,
            CertType = 123,
            Term = 365,
            Csr = "CSR-DATA"
        };

        var result = await client.EnrollAsync(request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/enroll", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal(5, body.GetProperty("orgId").GetInt32());
        Assert.Equal(123, body.GetProperty("certType").GetInt32());
        Assert.Equal(365, body.GetProperty("term").GetInt32());
        Assert.Equal("CSR-DATA", body.GetProperty("csr").GetString());

        Assert.NotNull(result);
        Assert.Equal(10, result!.SslId);
        Assert.Equal("r1", result.RenewId);
    }

    [Fact]
    public async Task EnrollWithKeyGenerationAsync_PostsToEnrollKeyGenEndpoint() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var enrollResponse = new AdminSslEnrollResponse {
            SslId = 20,
            RenewId = "r2"
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(enrollResponse)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var request = new AdminSslEnrollKeyGenRequest {
            OrgId = 7,
            CertType = 321,
            Term = 730,
            CommonName = "example.com"
        };

        var result = await client.EnrollWithKeyGenerationAsync(request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/enroll-keygen", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal(7, body.GetProperty("orgId").GetInt32());
        Assert.Equal(321, body.GetProperty("certType").GetInt32());
        Assert.Equal(730, body.GetProperty("term").GetInt32());
        Assert.Equal("example.com", body.GetProperty("commonName").GetString());

        Assert.NotNull(result);
        Assert.Equal(20, result!.SslId);
        Assert.Equal("r2", result.RenewId);
    }

    [Fact]
    public async Task ImportAsync_PostsMultipartToImportEndpoint() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var report = new ImportCertificateResponse {
            ProcessedCount = 3,
            Errors = new[] { "e1" }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(report)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        await using var ms = new MemoryStream(new byte[] { 1, 2, 3 });
        var result = await client.ImportAsync(15, ms, "certs.zip");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/import?orgId=15", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.IsType<MultipartFormDataContent>(handler.LastRequest.Content);

        Assert.NotNull(result);
        Assert.Equal(3, result!.ProcessedCount);
        Assert.Single(result.Errors);
        Assert.Equal("e1", result.Errors[0]);
    }

    [Fact]
    public async Task ListAsync_BuildsQueryAndParsesResponse() {
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

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var result = await client.ListAsync(5, 10);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2?size=5&position=10", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].SslId);
        Assert.Equal("example.com", result[0].CommonName);
    }

    [Fact]
    public async Task GetAsync_BuildsUriAndParsesDetails() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var details = new AdminSslCertificateDetails {
            Id = 2,
            CommonName = "example.org",
            SerialNumber = "ABC"
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(details)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var result = await client.GetAsync(2);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/2", handler.LastRequest!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
        Assert.Equal("example.org", result.CommonName);
        Assert.Equal("ABC", result.SerialNumber);
    }

    [Fact]
    public async Task RevokeByIdAsync_BuildsUriAndBody() {
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
        var client = new AdminSslClient(config, http);

        await client.RevokeByIdAsync(42, "1", "compromised");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/revoke/42", handler.LastRequest!.RequestUri!.ToString());
        Assert.False(string.IsNullOrWhiteSpace(handler.LastRequestBody));
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("1", body.GetProperty("reasonCode").GetString());
        Assert.Equal("compromised", body.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task RevokeBySerialAsync_BuildsUriAndBody() {
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
        var client = new AdminSslClient(config, http);

        await client.RevokeBySerialAsync("ABC", "3", "reason-text");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/revoke/serial/ABC", handler.LastRequest!.RequestUri!.ToString());
        Assert.False(string.IsNullOrWhiteSpace(handler.LastRequestBody));
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("3", body.GetProperty("reasonCode").GetString());
        Assert.Equal("reason-text", body.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task MarkAsRevokedAsync_BuildsUriAndBody() {
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
        var client = new AdminSslClient(config, http);

        var revokeDate = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);
        await client.MarkAsRevokedAsync(certId: 10, serialNumber: "ABC", issuer: "CN=Issuer", revokeDate: revokeDate, reasonCode: "4");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/revoke/manual", handler.LastRequest!.RequestUri!.ToString());
        Assert.False(string.IsNullOrWhiteSpace(handler.LastRequestBody));
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal(10, body.GetProperty("certId").GetInt32());
        Assert.Equal("ABC", body.GetProperty("serialNumber").GetString());
        Assert.Equal("CN=Issuer", body.GetProperty("issuer").GetString());
        Assert.Equal("4", body.GetProperty("reasonCode").GetString());
    }
}
