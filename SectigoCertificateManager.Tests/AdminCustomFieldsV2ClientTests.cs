using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Requests;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="AdminCustomFieldsV2Client"/>.
/// </summary>
public sealed class AdminCustomFieldsV2ClientTests {
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
    public async Task ListAsync_BuildsQueryAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var fields = new[] {
            new AdminCustomFieldV2 {
                Id = 1,
                Name = "Field1",
                CertType = "SSL",
                State = "ACTIVE",
                Input = new AdminCustomFieldInput { Type = "TEXT_SINGLE_LINE" },
                Mandatories = new[] { "REST_API" }
            }
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
        var client = new AdminCustomFieldsV2Client(config, http);

        var result = await client.ListAsync("SSL");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/customField/v2?certType=SSL", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("Field1", result[0].Name);
        Assert.Equal("SSL", result[0].CertType);
    }

    [Fact]
    public async Task GetAsync_BuildsUriAndParsesField() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var field = new AdminCustomFieldV2 {
            Id = 2,
            Name = "Field2",
            CertType = "Device",
            State = "INACTIVE"
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(field)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminCustomFieldsV2Client(config, http);

        var result = await client.GetAsync(2);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/customField/v2/2", handler.LastRequest!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(2, result!.Id);
        Assert.Equal("Field2", result.Name);
        Assert.Equal("Device", result.CertType);
    }

    [Fact]
    public async Task CreateAsync_PostsToEndpointAndParsesLocation() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.Created);
        apiResponse.Headers.Location = new Uri("https://admin.enterprise.sectigo.com/api/customField/v2/143");

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminCustomFieldsV2Client(config, http);

        var request = new CreateCustomFieldV2Request {
            Name = "NewField",
            CertType = "SSL",
            Input = new AdminCustomFieldInput { Type = "TEXT_SINGLE_LINE" },
            Mandatories = new[] { "REST_API" }
        };

        var id = await client.CreateAsync(request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/customField/v2", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("NewField", body.GetProperty("name").GetString());
        Assert.Equal("SSL", body.GetProperty("certType").GetString());
        Assert.Equal(143, id);
    }

    [Fact]
    public async Task UpdateAsync_PutsFieldAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var updated = new AdminCustomFieldV2 {
            Id = 10,
            Name = "UpdatedField",
            CertType = "SMIME",
            State = "ACTIVE"
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(updated)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminCustomFieldsV2Client(config, http);

        var field = new AdminCustomFieldV2 {
            Id = 10,
            Name = "UpdatedField",
            CertType = "SMIME",
            State = "ACTIVE"
        };

        var result = await client.UpdateAsync(field);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/customField/v2", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal(10, body.GetProperty("id").GetInt32());
        Assert.Equal("UpdatedField", body.GetProperty("name").GetString());

        Assert.NotNull(result);
        Assert.Equal(10, result!.Id);
        Assert.Equal("UpdatedField", result.Name);
    }

    [Fact]
    public async Task TokenIsCachedAcrossMultipleApiCalls() {
        var token = new { access_token = "tok", expires_in = 3600 };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var fields = new[] {
            new AdminCustomFieldV2 { Id = 1, Name = "Field1", CertType = "SSL" }
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
        var client = new AdminCustomFieldsV2Client(config, http);

        _ = await client.ListAsync("SSL");
        _ = await client.ListAsync("Device");

        Assert.Equal(1, handler.TokenRequestCount);
    }
}
