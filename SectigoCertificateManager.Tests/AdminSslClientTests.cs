using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Linq;
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
        private readonly string _tokenJson;
        private readonly HttpStatusCode _tokenStatus;
        private readonly string _apiJson;
        private readonly HttpStatusCode _apiStatus;
        private readonly Uri? _apiLocation;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }
        public int TokenRequestCount { get; private set; }

        public TestHandler(HttpResponseMessage tokenResponse, HttpResponseMessage apiResponse) {
            _tokenStatus = tokenResponse.StatusCode;
            _tokenJson = tokenResponse.Content is null
                ? string.Empty
                : tokenResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _apiStatus = apiResponse.StatusCode;
            _apiLocation = apiResponse.Headers.Location;
            _apiJson = apiResponse.Content is null
                ? string.Empty
                : apiResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            LastRequest = request;

            if (request.Content is not null) {
                LastRequestBody = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            } else {
                LastRequestBody = null;
            }

            if (request.RequestUri!.AbsoluteUri.Contains("protocol/openid-connect/token")) {
                TokenRequestCount++;
                var tokenResponse = new HttpResponseMessage(_tokenStatus) {
                    Content = new StringContent(_tokenJson, System.Text.Encoding.UTF8, "application/json")
                };
                return tokenResponse;
            }

            var apiResponse = new HttpResponseMessage(_apiStatus);
            if (!string.IsNullOrEmpty(_apiJson)) {
                apiResponse.Content = new StringContent(_apiJson, System.Text.Encoding.UTF8, "application/json");
            }
            if (_apiLocation is not null) {
                apiResponse.Headers.Location = _apiLocation;
            }
            return apiResponse;
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

        using var ms = new MemoryStream(new byte[] { 1, 2, 3 });
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
    public async Task RenewByRenewIdAsync_BuildsUriAndBody() {
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

        var request = new RenewCertificateRequest {
            Csr = "CSR-DATA",
            DcvMode = "EMAIL",
            DcvEmail = "admin@example.com"
        };

        await client.RenewByRenewIdAsync("renew-1", request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/renew/renew-1", handler.LastRequest!.RequestUri!.ToString());
        Assert.False(string.IsNullOrWhiteSpace(handler.LastRequestBody));
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("CSR-DATA", body.GetProperty("csr").GetString());
        Assert.Equal("EMAIL", body.GetProperty("dcvMode").GetString());
        Assert.Equal("admin@example.com", body.GetProperty("dcvEmail").GetString());
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

    [Fact]
    public async Task ApproveAsync_BuildsUriAndBody() {
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

        await client.ApproveAsync(99, "approved");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/approve/99", handler.LastRequest!.RequestUri!.ToString());
        Assert.False(string.IsNullOrWhiteSpace(handler.LastRequestBody));
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("approved", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task DeclineAsync_BuildsUriAndBody() {
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

        await client.DeclineAsync(77, "declined");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/decline/77", handler.LastRequest!.RequestUri!.ToString());
        Assert.False(string.IsNullOrWhiteSpace(handler.LastRequestBody));
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("declined", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ListCertificateTypesAsync_ReturnsTypes() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var types = new[] {
            new CertificateType {
                Id = 1,
                Name = "SSL Profile 1",
                Description = "Profile",
                Terms = new[] { 365 },
                KeyTypes = new Dictionary<string, IReadOnlyList<string>> {
                    ["RSA"] = new[] { "2048" }
                },
                UseSecondaryOrgName = true
            }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(types)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var result = await client.ListCertificateTypesAsync();

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/types", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("SSL Profile 1", result[0].Name);
    }

    [Fact]
    public async Task ListCertificateTypesAsync_WithOrganizationId_AppendsQuery() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var types = Array.Empty<CertificateType>();
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(types)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        _ = await client.ListCertificateTypesAsync(5);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/types?organizationId=5", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task ListCustomFieldsAsync_ReturnsFields() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var fields = new[] {
            new CustomField { Id = 10, Name = "Field1", Mandatory = true }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(fields)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var result = await client.ListCustomFieldsAsync();

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/customFields", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
        Assert.True(result[0].Mandatory);
    }

    [Fact]
    public async Task GetDcvInfoAsync_BuildsUriAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var dcvInfos = new[] {
            new AdminSslDcvInfo {
                DcvLog = new AdminSslDcvLog {
                    Error = new AdminSslDcvErrorDetails { Code = 1, Description = "err" },
                    Log = new[] { new AdminSslDcvLogEntry { DcvStatus = "Pending", DomainName = "example.com" } }
                },
                Instructions = new[] { new AdminSslDcvInstruction { DomainName = "example.com", DcvMode = "EMAIL" } }
            }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(dcvInfos)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var result = await client.GetDcvInfoAsync(10);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/10/dcv", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.NotNull(result[0].DcvLog);
        Assert.Equal("example.com", result[0].Instructions[0].DomainName);
    }

    [Fact]
    public async Task RecheckDcvAsync_BuildsUriAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var info = new AdminSslDcvInfo {
            DcvLog = new AdminSslDcvLog(),
            Instructions = new[] { new AdminSslDcvInstruction { DomainName = "example.org", DcvMode = "TXT" } }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.Accepted) {
            Content = JsonContent.Create(info)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var result = await client.RecheckDcvAsync(11);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/11/dcv/recheck", handler.LastRequest!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal("example.org", result!.Instructions[0].DomainName);
    }

    [Fact]
    public async Task ListLocationsAsync_BuildsUriAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var locations = new[] {
            new AdminSslLocation {
                Id = 42,
                LocationType = "CUSTOM",
                Name = "loc1",
                Details = new Dictionary<string, string> { ["key"] = "value" }
            }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(locations)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var result = await client.ListLocationsAsync(5);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/5/location", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(42, result[0].Id);
        Assert.Equal("loc1", result[0].Name);
    }

    [Fact]
    public async Task GetLocationAsync_BuildsUriAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var location = new AdminSslLocation {
            Id = 7,
            LocationType = "CUSTOM",
            Name = "loc2",
            Details = new Dictionary<string, string> { ["k"] = "v" }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(location)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var result = await client.GetLocationAsync(6, 7);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/6/location/7", handler.LastRequest!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
    }

    [Fact]
    public async Task CreateLocationAsync_ReturnsIdFromLocationHeader() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.Created);
        apiResponse.Headers.Location = new Uri("https://admin.enterprise.sectigo.com/api/ssl/v2/5/location/99");

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminSslClient(config, http);

        var request = new AdminSslLocationRequest {
            Details = new Dictionary<string, string> { ["k"] = "v" }
        };

        var id = await client.CreateLocationAsync(5, request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/ssl/v2/5/location", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.Equal(99, id);
    }

    [Fact]
    public async Task TokenIsCachedAcrossMultipleApiCalls() {
        var token = new { access_token = "tok", expires_in = 3600 };
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

        // Two separate API calls should share the same cached token.
        _ = await client.ListAsync(5, 0);
        _ = await client.ListAsync(10, 5);

        Assert.Equal(1, handler.TokenRequestCount);
    }

    [Fact]
    public async Task ConcurrentRequests_OnlyRefreshesTokenOnce() {
        var token = new { access_token = "tok", expires_in = 3600 };
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

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => client.ListAsync(5, 0))
            .ToArray();

        await Task.WhenAll(tasks);

        Assert.Equal(1, handler.TokenRequestCount);
    }
}
