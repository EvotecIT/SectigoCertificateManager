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

public sealed class AdminAcmePublicClientTests {
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
    public async Task ListAccountsAsync_BuildsQueryAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var accounts = new[] {
            new AdminPublicAcmeAccount {
                Id = 1,
                Name = "acct",
                AcmeServer = "server",
                OrganizationId = 10
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
        var client = new AdminAcmePublicClient(config, http);

        var result = await client.ListAccountsAsync(
            size: 5,
            position: 10,
            organizationId: 10,
            certValidationType: "DV",
            name: "acct",
            acmeServer: "server");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/acme/v2/account?size=5&position=10&organizationId=10&certValidationType=DV&name=acct&acmeServer=server",
            handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal(10, result[0].OrganizationId);
    }

    [Fact]
    public async Task CreateAccountAsync_PostsPayloadAndParsesLocation() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.Created);
        apiResponse.Headers.Location = new System.Uri("https://admin.enterprise.sectigo.com/api/acme/v2/account/42");

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminAcmePublicClient(config, http);

        var request = new AdminPublicAcmeAccountCreateRequest {
            AcmeServer = "server",
            Name = "acct",
            OrganizationId = 10,
            CertValidationType = "DV"
        };

        var id = await client.CreateAccountAsync(request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/acme/v2/account", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        Assert.Equal("server", root.GetProperty("acmeServer").GetString());
        Assert.Equal("acct", root.GetProperty("name").GetString());
        Assert.Equal(10, root.GetProperty("organizationId").GetInt32());

        Assert.Equal(42, id);
    }

    [Fact]
    public async Task ListDomainsAsync_BuildsUri() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var domains = new[] {
            new AdminPublicAcmeDomain { Name = "example.com", ValidUntil = "2026-01-01" }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(domains)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminAcmePublicClient(config, http);

        var result = await client.ListDomainsAsync(
            7,
            size: 20,
            position: 0,
            name: "example",
            expiresWithinNextDays: 30,
            stickyExpiresWithinNextDays: 60);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/acme/v2/account/7/domain?size=20&position=0&name=example&expiresWithinNextDays=30&stickyExpiresWithinNextDays=60",
            handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal("example.com", result[0].Name);
    }

    [Fact]
    public async Task AddDomainsAsync_PostsListAndParsesNotAdded() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new {
                notAddedDomains = new[] { "bad.example.com" }
            })
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminAcmePublicClient(config, http);

        var domains = new[] {
            new AdminAcmeDomainNameRequest { Name = "example.com" }
        };

        var result = await client.AddDomainsAsync(7, domains);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/acme/v2/account/7/domain", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var root = doc.RootElement;
        var list = root.GetProperty("domains");
        Assert.Equal("example.com", list[0].GetProperty("name").GetString());

        Assert.Single(result);
        Assert.Equal("bad.example.com", result[0]);
    }

    [Fact]
    public async Task RemoveDomainsAsync_UsesDeleteAndParsesNotRemoved() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new {
                notRemovedDomains = new[] { "locked.example.com" }
            })
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminAcmePublicClient(config, http);

        var domains = new[] {
            new AdminAcmeDomainNameRequest { Name = "example.com" }
        };

        var result = await client.RemoveDomainsAsync(11, domains);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/acme/v2/account/11/domain", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Delete, handler.LastRequest.Method);

        Assert.Single(result);
        Assert.Equal("locked.example.com", result[0]);
    }

    [Fact]
    public async Task ListClientsAsync_BuildsQuery() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var clients = new[] {
            new AdminPublicAcmeClient { Id = 1, AccountId = "acc", Status = "ACTIVE" }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(clients)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminAcmePublicClient(config, http);

        var result = await client.ListClientsAsync(
            9,
            size: 10,
            position: 0,
            contacts: "admin@example.com",
            userAgent: "acme-client",
            ipAddress: "192.0.2.1",
            lastActivityWithinPrevDays: 7);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/acme/v2/account/9/client?size=10&position=0&contacts=admin%40example.com&userAgent=acme-client&ipAddress=192.0.2.1&lastActivityWithinPrevDays=7",
            handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task DeactivateClientAsync_BuildsUri() {
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
        var client = new AdminAcmePublicClient(config, http);

        await client.DeactivateClientAsync(12, "client-1");

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/acme/v2/account/12/client/client-1", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
    }

    [Fact]
    public async Task TokenIsCachedAcrossMultipleApiCalls() {
        var token = new { access_token = "tok", expires_in = 3600 };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var accounts = new[] {
            new AdminPublicAcmeAccount { Id = 1, Name = "acct" }
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
        var client = new AdminAcmePublicClient(config, http);

        _ = await client.ListAccountsAsync(size: 5);
        _ = await client.ListAccountsAsync(size: 10);

        Assert.Equal(1, handler.TokenRequestCount);
    }
}
