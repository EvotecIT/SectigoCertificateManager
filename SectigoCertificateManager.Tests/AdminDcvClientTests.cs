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
/// Unit tests for <see cref="AdminDcvClient"/>.
/// </summary>
public sealed class AdminDcvClientTests {
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
            new AdminDcvValidationSummary {
                Domain = "example.com",
                DcvStatus = "VALIDATED",
                OrderStatus = "COMPLETED",
                Expires = "2026-01-01"
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
        var client = new AdminDcvClient(config, http);

        var result = await client.ListAsync(
            domain: "example.com",
            expiresInDays: 30,
            organizationId: 5,
            departmentId: 10,
            dcvStatus: "PENDING",
            orderStatus: "Submitted",
            size: 50,
            position: 0);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/dcv/v2/validation?domain=example.com&expiresIn=30&org=5&department=10&dcvStatus=PENDING&orderStatus=Submitted&size=50&position=0",
            handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal("example.com", result[0].Domain);
        Assert.Equal("VALIDATED", result[0].DcvStatus);
        Assert.Equal("COMPLETED", result[0].OrderStatus);
    }

    [Fact]
    public async Task GetStatusAsync_BuildsRequestAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var status = new AdminDcvStatus {
            Status = "VALIDATED",
            OrderStatus = "COMPLETED",
            OrderMode = "EMAIL",
            ValidationEmail = "admin@example.com"
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(status)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDcvClient(config, http);

        var result = await client.GetStatusAsync("example.com");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/dcv/v2/validation/status", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        Assert.Equal("example.com", root.GetProperty("domain").GetString());

        Assert.NotNull(result);
        Assert.Equal("VALIDATED", result!.Status);
        Assert.Equal("COMPLETED", result.OrderStatus);
        Assert.Equal("EMAIL", result.OrderMode);
        Assert.Equal("admin@example.com", result.ValidationEmail);
    }

    [Fact]
    public async Task ClearAsync_BuildsRequest() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDcvClient(config, http);

        await client.ClearAsync("example.com");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/dcv/v2/validation/clear", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        Assert.Equal("example.com", root.GetProperty("domain").GetString());
    }

    [Fact]
    public async Task DeleteAsync_BuildsRequest() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDcvClient(config, http);

        await client.DeleteAsync("example.com");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/dcv/v2/validation/delete", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        Assert.Equal("example.com", root.GetProperty("domain").GetString());
    }

    [Fact]
    public async Task TokenIsCachedAcrossMultipleApiCalls() {
        var token = new { access_token = "tok", expires_in = 3600 };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var items = new[] {
            new AdminDcvValidationSummary { Domain = "example.com", DcvStatus = "PENDING" }
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
        var client = new AdminDcvClient(config, http);

        _ = await client.ListAsync(domain: "example.com");
        _ = await client.ListAsync(domain: "example.org");

        Assert.Equal(1, handler.TokenRequestCount);
    }
}

