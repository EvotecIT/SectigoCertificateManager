using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Requests;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class AdminAcmePrivateClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly string _tokenJson;
        private readonly HttpStatusCode _tokenStatus;
        private readonly string _apiJson;
        private readonly HttpStatusCode _apiStatus;
        private readonly Uri? _apiLocation;

        public HttpRequestMessage? LastRequest { get; private set; }
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

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            LastRequest = request;

            if (request.RequestUri!.AbsoluteUri.Contains("protocol/openid-connect/token")) {
                TokenRequestCount++;
                var tokenResponse = new HttpResponseMessage(_tokenStatus) {
                    Content = new StringContent(_tokenJson, System.Text.Encoding.UTF8, "application/json")
                };
                return Task.FromResult(tokenResponse);
            }

            var apiResponse = new HttpResponseMessage(_apiStatus);
            if (!string.IsNullOrEmpty(_apiJson)) {
                apiResponse.Content = new StringContent(_apiJson, System.Text.Encoding.UTF8, "application/json");
            }
            if (_apiLocation is not null) {
                apiResponse.Headers.Location = _apiLocation;
            }
            return Task.FromResult(apiResponse);
        }
    }

    [Fact]
    public async Task ListAccountsAsync_BuildsQueryAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var accounts = new[] {
            new AdminPrivateAcmeAccount {
                Id = 1,
                Name = "pca",
                AcmeServer = "server",
                OrganizationId = 2
            }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(accounts)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminAcmePrivateClient(config, http);

        var result = await client.ListAccountsAsync(
            size: 5,
            position: 0,
            organizationId: 2,
            name: "pca",
            acmeServer: "server");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/acme/v1/pca/account?size=5&position=0&organizationId=2&name=pca&acmeServer=server",
            handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[0].OrganizationId);
    }

    [Fact]
    public async Task CreateAccountAsync_PostsPayloadAndParsesLocation() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.Created);
        apiResponse.Headers.Location = new System.Uri("https://admin.enterprise.sectigo.com/api/acme/v1/pca/account/99");

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminAcmePrivateClient(config, http);

        var request = new AdminPrivateAcmeAccountCreateRequest {
            AcmeServer = "server",
            Name = "pca",
            OrganizationId = 2,
            ProfileName = "profile"
        };

        var id = await client.CreateAccountAsync(request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/acme/v1/pca/account", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);

        Assert.Equal(99, id);
    }

    [Fact]
    public async Task TokenIsCachedAcrossMultipleApiCalls() {
        var token = new { access_token = "tok", expires_in = 3600 };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var accounts = new[] {
            new AdminPrivateAcmeAccount { Id = 1, Name = "pca" }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(accounts)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminAcmePrivateClient(config, http);

        _ = await client.ListAccountsAsync(size: 5);
        _ = await client.ListAccountsAsync(size: 10);

        Assert.Equal(1, handler.TokenRequestCount);
    }
}
