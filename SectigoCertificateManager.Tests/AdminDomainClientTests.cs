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
/// Unit tests for <see cref="AdminDomainClient"/>.
/// </summary>
public sealed class AdminDomainClientTests {
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

        var domains = new[] {
            new AdminDomainInfo {
                Id = 1,
                Name = "example.com",
                State = "ACTIVE",
                DelegationStatus = "ACTIVE"
            }
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
        var client = new AdminDomainClient(config, http);

        var result = await client.ListAsync(size: 10, position: 5, name: "example.com", state: "active", status: "requested", orgId: 42);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/domain/v1?size=10&position=5&name=example.com&state=active&status=requested&orgId=42", handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
        Assert.Equal("example.com", result[0].Name);
    }

    [Fact]
    public async Task CreateAsync_PostsToEndpointAndParsesLocation() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.Created);
        apiResponse.Headers.Location = new Uri("https://admin.enterprise.sectigo.com/api/domain/v1/123");

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDomainClient(config, http);

        var request = new CreateDomainRequest {
            Name = "example.com",
            Description = "Test domain",
            Active = "true",
            Delegations = new[] {
                new AdminDomainDelegation {
                    OrgId = 10,
                    CertTypes = new[] { "SSL" },
                    DomainCertificateRequestPrivileges = new[] { "ENROLL" }
                }
            }
        };

        var id = await client.CreateAsync(request);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/domain/v1", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal("example.com", body.GetProperty("name").GetString());
        Assert.Equal("true", body.GetProperty("active").GetString());
        Assert.Equal(123, id);
    }

    [Fact]
    public async Task DelegateAsync_PostsDelegationPayload() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new { ok = true })
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDomainClient(config, http);

        var delegation = new AdminDomainDelegation {
            OrgId = 10,
            CertTypes = new[] { "SSL" },
            DomainCertificateRequestPrivileges = new[] { "ENROLL" }
        };

        await client.DelegateAsync(5, delegation);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/domain/v1/5/delegation", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal(10, body.GetProperty("orgId").GetInt32());
    }

    [Fact]
    public async Task ApproveDelegationAsync_PostsApprovePayload() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new { ok = true })
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDomainClient(config, http);

        await client.ApproveDelegationAsync(7, 10);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/domain/v1/7/delegation/approve", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Post, handler.LastRequest.Method);
        Assert.NotNull(handler.LastRequestBody);
        using var doc = JsonDocument.Parse(handler.LastRequestBody!);
        var body = doc.RootElement;
        Assert.Equal(10, body.GetProperty("orgId").GetInt32());
    }

    [Fact]
    public async Task SuspendAsync_PutsToSuspendEndpoint() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new { ok = true })
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");
        var client = new AdminDomainClient(config, http);

        await client.SuspendAsync(8);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/domain/v1/8/suspend", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal(HttpMethod.Put, handler.LastRequest.Method);
    }

    [Fact]
    public async Task TokenIsCachedAcrossMultipleApiCalls() {
        var token = new { access_token = "tok", expires_in = 3600 };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var domains = new[] {
            new AdminDomainInfo { Id = 1, Name = "example.com" }
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
        var client = new AdminDomainClient(config, http);

        _ = await client.ListAsync(size: 10);
        _ = await client.ListAsync(size: 5, position: 10);

        Assert.Equal(1, handler.TokenRequestCount);
    }
}
