using SectigoCertificateManager.AdminApi;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class AdminAdminClientTests {
    private static AdminApiConfig CreateConfig() {
        return new AdminApiConfig(
            "https://admin.enterprise.sectigo.com/",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "client-id",
            "client-secret");
    }

    [Fact]
    public async Task ListAsync_BuildsQueryAndParsesResponse() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var itemsPayload = "[{\"id\":1,\"type\":\"STANDARD\",\"login\":\"admin\",\"forename\":\"Alice\",\"surname\":\"Admin\",\"email\":\"alice@example.com\"}]";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(itemsPayload, Encoding.UTF8, "application/json")
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        var items = await client.ListAsync(
            size: 10,
            position: 5,
            login: "admin",
            email: "admin@example.com",
            activeState: AdminActiveState.Active,
            orgId: 123,
            type: AdminAccountType.Standard,
            templateId: 42,
            identityProviderId: 9,
            role: "MRAO",
            cancellationToken: CancellationToken.None);

        Assert.Equal("https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token", handler.TokenRequest?.RequestUri?.ToString());

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/admin/v1?size=10&position=5&login=admin&email=admin%40example.com&activeState=ACTIVE&orgId=123&type=STANDARD&templateId=42&identityProviderId=9&role=MRAO",
            handler.ApiRequest.RequestUri!.ToString());

        Assert.Single(items);
        Assert.Equal(1, items[0].Id);
        Assert.Equal("admin", items[0].Login);
        Assert.Equal("Alice", items[0].Forename);
        Assert.Equal("Admin", items[0].Surname);
        Assert.Equal("alice@example.com", items[0].Email);
    }

    [Fact]
    public async Task GetAsync_BuildsUriAndParsesDetails() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var payload = @"{
  ""id"": 99,
  ""type"": ""API"",
  ""forename"": ""Alice"",
  ""surname"": ""Admin"",
  ""login"": ""api-admin"",
  ""email"": ""alice@example.com"",
  ""passwordState"": ""EXPIRED"",
  ""passwordExpiryDate"": ""2025-12-31"",
  ""activeState"": ""ACTIVE"",
  ""identityProviderId"": 7,
  ""idp"": ""AzureAD"",
  ""templateId"": 12
}";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        var details = await client.GetAsync(99, CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/99", handler.ApiRequest.RequestUri!.ToString());

        Assert.NotNull(details);
        Assert.Equal(99, details!.Id);
        Assert.Equal("api-admin", details.Login);
        Assert.Equal("Alice", details.Forename);
        Assert.Equal("Admin", details.Surname);
        Assert.Equal("alice@example.com", details.Email);
        Assert.Equal(AdminActiveState.Active, details.ActiveState);
        Assert.Equal(AdminPasswordState.EXPIRED, details.PasswordState);
        Assert.Equal(7, details.IdentityProviderId);
        Assert.Equal("AzureAD", details.Idp);
        Assert.Equal(12, details.TemplateId);
    }

    [Fact]
    public async Task CreateAsync_PostsPayloadAndParsesLocation() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.Created);
        apiResponse.Headers.Location = new Uri("https://admin.enterprise.sectigo.com/api/admin/v1/42");

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);
        var request = new AdminCreateOrUpdateRequest {
            Type = AdminAccountType.Api,
            Login = "api-admin",
            Email = "alice@example.com",
            Forename = "Alice",
            Surname = "Admin"
        };

        var id = await client.CreateAsync(request, CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Post, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1", handler.ApiRequest.RequestUri!.ToString());

        using var doc = JsonDocument.Parse(handler.Body!);
        var root = doc.RootElement;
        Assert.Equal("API", root.GetProperty("type").GetString());
        Assert.Equal("api-admin", root.GetProperty("login").GetString());
        Assert.Equal("alice@example.com", root.GetProperty("email").GetString());

        Assert.Equal(42, id);
    }

    [Fact]
    public async Task UpdateAsync_PutsPayload() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK);

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);
        var request = new AdminCreateOrUpdateRequest {
            Forename = "Alice",
            Surname = "Updated"
        };

        await client.UpdateAsync(99, request, CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Put, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/99", handler.ApiRequest.RequestUri!.ToString());

        using var doc = JsonDocument.Parse(handler.Body!);
        var root = doc.RootElement;
        Assert.Equal("Alice", root.GetProperty("forename").GetString());
        Assert.Equal("Updated", root.GetProperty("surname").GetString());
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteWithQueryWhenReplacingRequesterProvided() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        await client.DeleteAsync(10, replacingRequesterId: 5, cancellationToken: CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Delete, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/10?replacingRequesterId=5", handler.ApiRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task UnlinkFromTemplateAsync_BuildsUri() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK);

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        await client.UnlinkFromTemplateAsync(15, CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Put, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/15/unlink", handler.ApiRequest.RequestUri!.ToString());
    }

    [Fact]
    public async Task ChangeOwnPasswordAsync_PostsHeaderAndBody() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.NoContent);

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        await client.ChangeOwnPasswordAsync("old-pass", "new-pass", CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Post, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/changepassword", handler.ApiRequest.RequestUri!.ToString());

        Assert.True(handler.ApiRequest.Headers.TryGetValues("password", out var values));
        Assert.Contains("old-pass", values);

        using var doc = JsonDocument.Parse(handler.Body!);
        var root = doc.RootElement;
        Assert.Equal("new-pass", root.GetProperty("newPassword").GetString());
    }

    [Fact]
    public async Task GetPasswordStatusAsync_ReturnsStatus() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var payload = "{\"expirationDate\":\"2025-12-31\",\"state\":\"EXPIRED\"}";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        var status = await client.GetPasswordStatusAsync(CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/password", handler.ApiRequest.RequestUri!.ToString());

        Assert.NotNull(status);
        Assert.Equal(AdminPasswordState.EXPIRED, status!.State);
        Assert.Equal(2025, status.ExpirationDate?.Year);
        Assert.Equal(12, status.ExpirationDate?.Month);
        Assert.Equal(31, status.ExpirationDate?.Day);
    }

    [Fact]
    public async Task ListRolesAsync_BuildsQueryAndParsesResponse() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var payload = "[\"MRAO\",\"RAO_SSL\"]";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        var roles = await client.ListRolesAsync(isForEdit: true, cancellationToken: CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/roles?isForEdit=true", handler.ApiRequest.RequestUri!.ToString());

        Assert.Equal(2, roles.Count);
        Assert.Equal(AdminRole.MRAO, roles[0]);
        Assert.Equal(AdminRole.RAO_SSL, roles[1]);
    }

    [Fact]
    public async Task ListPrivilegesAsync_BuildsQueryAndParsesResponse() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var payload = "[{\"name\":\"autoApproveCertificates\",\"description\":\"Automatically approve certificate requests\"}]";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        var privileges = await client.ListPrivilegesAsync(
            new[] { AdminRole.MRAO, AdminRole.RAO_SSL },
            CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/privileges?role=MRAO&role=RAO_SSL", handler.ApiRequest.RequestUri!.ToString());

        Assert.Single(privileges);
        Assert.Equal("autoApproveCertificates", privileges[0].Name);
        Assert.Equal("Automatically approve certificate requests", privileges[0].Description);
    }

    [Fact]
    public async Task ListIdentityProvidersAsync_BuildsUriAndParsesResponse() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var payload = "[{\"id\":5,\"name\":\"AzureAD\"}]";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(payload, Encoding.UTF8, "application/json")
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAdminClient(CreateConfig(), httpClient);

        var items = await client.ListIdentityProvidersAsync(CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/admin/v1/idp", handler.ApiRequest.RequestUri!.ToString());

        Assert.Single(items);
        Assert.Equal(5, items[0].Id);
        Assert.Equal("AzureAD", items[0].Name);
    }

    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _tokenResponse;
        private readonly HttpResponseMessage _apiResponse;

        public HttpRequestMessage? TokenRequest { get; private set; }

        public HttpRequestMessage? ApiRequest { get; private set; }
        public string? Body { get; private set; }

        public TestHandler(HttpResponseMessage tokenResponse, HttpResponseMessage apiResponse) {
            _tokenResponse = tokenResponse ?? throw new ArgumentNullException(nameof(tokenResponse));
            _apiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (request.RequestUri!.AbsoluteUri.IndexOf("protocol/openid-connect/token", StringComparison.OrdinalIgnoreCase) >= 0) {
                TokenRequest = request;
                return _tokenResponse;
            }

            ApiRequest = request;
            if (request.Content is not null) {
                Body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            return _apiResponse;
        }
    }
}
