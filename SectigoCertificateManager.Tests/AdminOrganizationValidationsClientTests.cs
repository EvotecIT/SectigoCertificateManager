using SectigoCertificateManager.AdminApi;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="AdminOrganizationValidationsClient"/>.
/// </summary>
public sealed class AdminOrganizationValidationsClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly string _tokenJson;
        private readonly HttpStatusCode _tokenStatus;
        private readonly string _apiJson;
        private readonly HttpStatusCode _apiStatus;

        public HttpRequestMessage? LastRequest { get; private set; }
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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            LastRequest = request;

            if (request.RequestUri!.AbsoluteUri.Contains("protocol/openid-connect/token")) {
                TokenRequestCount++;
                var tokenResponse = new HttpResponseMessage(_tokenStatus) {
                    Content = new StringContent(_tokenJson, System.Text.Encoding.UTF8, "application/json")
                };
                return Task.FromResult(tokenResponse);
            }

            var apiResponse = new HttpResponseMessage(_apiStatus) {
                Content = new StringContent(_apiJson, System.Text.Encoding.UTF8, "application/json")
            };
            return Task.FromResult(apiResponse);
        }
    }

    [Fact]
    public async Task ListAsync_BuildsUriAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var items = new[] {
            new AdminValidationSummary {
                Id = 5,
                Type = "OV",
                Status = "VALIDATED",
                BackendType = "SASP"
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
        var client = new AdminOrganizationValidationsClient(config, http);

        var result = await client.ListAsync(10);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/organization/v2/10/validations", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(5, result[0].Id);
        Assert.Equal("OV", result[0].Type);
        Assert.Equal("VALIDATED", result[0].Status);
    }

    [Fact]
    public async Task GetAsync_BuildsUriAndParsesDetails() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var details = new AdminValidationDetails {
            Id = 7,
            ValidationLevel = "EV_SSL",
            Status = "PENDING",
            BackendType = "SASP"
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
        var client = new AdminOrganizationValidationsClient(config, http);

        var result = await client.GetAsync(20, 7);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/organization/v2/20/validations/7", handler.LastRequest!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(7, result!.Id);
        Assert.Equal("EV_SSL", result.ValidationLevel);
        Assert.Equal("PENDING", result.Status);
    }

    [Fact]
    public async Task SyncAsync_BuildsUriAndParsesDetails() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var details = new AdminValidationDetails {
            Id = 8,
            ValidationLevel = "OV_SSL",
            Status = "VALIDATED"
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
        var client = new AdminOrganizationValidationsClient(config, http);

        var result = await client.SyncAsync(30, 8);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/organization/v2/30/validations/8/sync", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(result);
        Assert.Equal(8, result!.Id);
        Assert.Equal("OV_SSL", result.ValidationLevel);
        Assert.Equal("VALIDATED", result.Status);
    }

    [Fact]
    public async Task DeleteAsync_BuildsUriAndSendsDelete() {
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
        var client = new AdminOrganizationValidationsClient(config, http);

        await client.DeleteAsync(40, 9);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/organization/v2/40/validations/9", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);
    }

    [Fact]
    public async Task TokenIsCachedAcrossMultipleApiCalls() {
        var token = new { access_token = "tok", expires_in = 3600 };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var items = new[] {
            new AdminValidationSummary { Id = 1, Type = "OV", Status = "PENDING" }
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
        var client = new AdminOrganizationValidationsClient(config, http);

        _ = await client.ListAsync(50);
        _ = await client.ListAsync(50);

        Assert.Equal(1, handler.TokenRequestCount);
    }
}

