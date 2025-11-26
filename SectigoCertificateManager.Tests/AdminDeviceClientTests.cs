using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Models;
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

/// <summary>
/// Unit tests for <see cref="AdminDeviceClient"/>.
/// </summary>
public sealed class AdminDeviceClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly string _tokenJson;
        private readonly HttpStatusCode _tokenStatus;
        private readonly string _apiJson;
        private readonly HttpStatusCode _apiStatus;

        public HttpRequestMessage? LastRequest { get; private set; }
        public string? LastRequestBody { get; private set; }
        public int TokenRequestCount { get; private set; }

        public TestHandler(HttpResponseMessage tokenResponse, HttpResponseMessage apiResponse) {
            _tokenStatus = tokenResponse.StatusCode;
            _tokenJson = tokenResponse.Content is null
                ? string.Empty
                : tokenResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            _apiStatus = apiResponse.StatusCode;
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

            var apiResponse = new HttpResponseMessage(_apiStatus) {
                Content = new StringContent(_apiJson, System.Text.Encoding.UTF8, "application/json")
            };
            return apiResponse;
        }
    }

    [Fact]
    public async Task ListAsync_BuildsQueryAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var items = new[] {
            new AdminDeviceIdentity {
                Id = 1,
                Status = "Issued",
                BackendCertId = "dev-1",
                CertificateDetails = new AdminDeviceCertificateDetails {
                    Subject = "CN=device1",
                    Sha256Hash = "ABC"
                }
            }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(items)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDeviceClient(config, http);

        var result = await client.ListAsync(5, 10);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1?size=5&position=10", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Issued", result[0].Status);
        Assert.NotNull(result[0].CertificateDetails);
        Assert.Equal("CN=device1", result[0].CertificateDetails!.Subject);
    }

    [Fact]
    public async Task GetAsync_BuildsUriAndParsesDetails() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var details = new AdminSslCertificateDetails {
            Id = 2,
            CommonName = "device.example.com",
            OrgId = 5,
            Status = "Issued",
            BackendCertId = "dev-2",
            Term = 365
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
        var client = new AdminDeviceClient(config, http);

        var result = await client.GetAsync(2);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/2", handler.LastRequest!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
        Assert.Equal("device.example.com", result.CommonName);
    }

    [Fact]
    public async Task EnrollAsync_PostsToEnrollEndpoint() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var enrollResponse = new AdminDeviceEnrollResponse {
            DeviceCertId = 42,
            Status = "Requested"
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
        var client = new AdminDeviceClient(config, http);

        var request = new DeviceEnrollRequest {
            OrgId = 5,
            Term = 365,
            Csr = "CSR-DATA",
            CertType = 123
        };

        var result = await client.EnrollAsync(request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/enroll", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal(5, body.GetProperty("orgId").GetInt32());
        Assert.Equal(365, body.GetProperty("term").GetInt32());
        Assert.Equal("CSR-DATA", body.GetProperty("csr").GetString());
        Assert.Equal(123, body.GetProperty("certType").GetInt32());

        Assert.NotNull(result);
        Assert.Equal(42, result!.DeviceCertId);
        Assert.Equal("Requested", result.Status);
    }

    [Fact]
    public async Task RenewByIdAsync_BuildsUriAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var enrollResponse = new AdminDeviceEnrollResponse {
            DeviceCertId = 100,
            Status = "Issued"
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
        var client = new AdminDeviceClient(config, http);

        var result = await client.RenewByIdAsync(100);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/renew/order/100", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(result);
        Assert.Equal(100, result!.DeviceCertId);
        Assert.Equal("Issued", result.Status);
    }

    [Fact]
    public async Task RenewBySerialAsync_BuildsUriAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var enrollResponse = new AdminDeviceEnrollResponse {
            DeviceCertId = 101,
            Status = "Issued"
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
        var client = new AdminDeviceClient(config, http);

        var result = await client.RenewBySerialAsync("ABC123");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/renew/serial/ABC123", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(result);
        Assert.Equal(101, result!.DeviceCertId);
        Assert.Equal("Issued", result.Status);
    }

    [Fact]
    public async Task ReplaceAsync_BuildsUriAndBody() {
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
        var client = new AdminDeviceClient(config, http);

        var request = new AdminSslReplaceRequest {
            Csr = "CSR-DATA",
            Reason = "Replace due to key compromise",
            CommonName = "device.example.com",
            SubjectAlternativeNames = new[] { "alt1.example.com", "alt2.example.com" },
            DcvMode = "EMAIL",
            DcvEmail = "admin@example.com"
        };

        await client.ReplaceAsync(42, request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/replace/order/42", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("CSR-DATA", body.GetProperty("csr").GetString());
        Assert.Equal("Replace due to key compromise", body.GetProperty("reason").GetString());
        Assert.Equal("device.example.com", body.GetProperty("commonName").GetString());
        var sans = body.GetProperty("subjectAlternativeNames");
        Assert.Equal(JsonValueKind.Array, sans.ValueKind);
        Assert.Equal("alt1.example.com", sans[0].GetString());
        Assert.Equal("alt2.example.com", sans[1].GetString());
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
        var client = new AdminDeviceClient(config, http);

        await client.RevokeBySerialAsync("ABC123", reasonCode: "1", reason: "Key compromise");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/revoke/serial/ABC123", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("1", body.GetProperty("reasonCode").GetString());
        Assert.Equal("Key compromise", body.GetProperty("reason").GetString());
    }

    [Fact]
    public async Task ImportAsync_PostsJsonArrayToImportEndpoint() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var results = new[] {
            new CertificateImportResult {
                Successful = true,
                BackendCertId = "dev-1",
                Cert = new CertificateIdentity {
                    Id = 1,
                    Subject = "CN=device1",
                    SerialNumber = "ABC123"
                },
                Created = true
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
        var client = new AdminDeviceClient(config, http);

        var requests = new[] {
            new CertificateImportRequest {
                OrgId = 5,
                Cert = "BASE64-CERT",
                BackendCertId = "dev-1",
                Owner = "owner@example.com"
            }
        };

        var result = await client.ImportAsync(requests);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/import", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        Assert.Equal(JsonValueKind.Array, root.ValueKind);
        var first = root[0];
        Assert.Equal(5, first.GetProperty("orgID").GetInt32());
        Assert.Equal("BASE64-CERT", first.GetProperty("cert").GetString());
        Assert.Equal("dev-1", first.GetProperty("backendCertId").GetString());

        Assert.Single(result);
        Assert.True(result[0].Successful);
        Assert.Equal("dev-1", result[0].BackendCertId);
        Assert.NotNull(result[0].Cert);
        var cert = result[0].Cert!;
        Assert.Equal(1, cert.Id);
        Assert.Equal("CN=device1", cert.Subject);
        Assert.Equal("ABC123", cert.SerialNumber);
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
        var client = new AdminDeviceClient(config, http);

        await client.ApproveAsync(10, "looks good");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/approve/10", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("looks good", body.GetProperty("message").GetString());
    }

    [Fact]
    public async Task ListCertificateTypesAsync_ReturnsTypes() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var types = new[] {
            new CertificateType { Id = 1, Name = "Device Profile 1" }
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
        var client = new AdminDeviceClient(config, http);

        var result = await client.ListCertificateTypesAsync();

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/types", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Device Profile 1", result[0].Name);
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
        var client = new AdminDeviceClient(config, http);

        var result = await client.ListCustomFieldsAsync();

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/customFields", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(10, result[0].Id);
        Assert.True(result[0].Mandatory);
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
        var client = new AdminDeviceClient(config, http);

        var result = await client.ListLocationsAsync(5);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/device/v1/5/location", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(42, result[0].Id);
        Assert.Equal("loc1", result[0].Name);
    }

    [Fact]
    public async Task TokenIsCachedAcrossMultipleApiCalls() {
        var token = new { access_token = "tok", expires_in = 3600 };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var items = new[] {
            new AdminDeviceIdentity { Id = 1, Status = "Issued", BackendCertId = "dev-1" }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(items)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDeviceClient(config, http);

        _ = await client.ListAsync(5, 0);
        _ = await client.ListAsync(10, 5);

        Assert.Equal(1, handler.TokenRequestCount);
    }
}
