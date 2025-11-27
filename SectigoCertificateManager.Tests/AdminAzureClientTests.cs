using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Utilities;
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

public sealed class AdminAzureClientTests {
    private static AdminApiConfig CreateConfig() {
        return new AdminApiConfig(
            "https://admin.enterprise.sectigo.com/",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "client-id",
            "client-secret");
    }

    [Fact]
    public async Task ListAccountsAsync_BuildsQueryAndParsesResponse() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var itemsPayload = "[{\"name\":\"acc1\",\"applicationId\":\"app\",\"directoryId\":\"dir\",\"id\":1,\"delegationMode\":\"GLOBAL_FOR_CUSTOMER\"}]";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(itemsPayload, Encoding.UTF8, "application/json")
        };
        apiResponse.Headers.Add("X-Total-Count", "10");

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAzureClient(CreateConfig(), httpClient);

        var (items, total) = await client.ListAccountsAsync(5, 10, CancellationToken.None).ConfigureAwait(false);

        Assert.Equal("https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token", handler.TokenRequest?.RequestUri?.ToString());

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/azure/v1/accounts?size=5&position=10", handler.ApiRequest.RequestUri!.ToString());

        Assert.Single(items);
        Assert.Equal("acc1", items[0].Name);
        Assert.Equal(1, items[0].Id);
        Assert.Equal(AzureDelegationMode.GlobalForCustomer, items[0].DelegationMode);
        Assert.Equal(10, total);
    }

    [Fact]
    public async Task CreateAccountAsync_PostsPayloadAndParsesLocation() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"abc\",\"expires_in\":300}")
        };

        var apiResponse = new HttpResponseMessage(HttpStatusCode.Created);
        apiResponse.Headers.Location = new Uri("https://admin.enterprise.sectigo.com/api/azure/v1/accounts/42");

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminAzureClient(CreateConfig(), httpClient);
        var request = new AzureAccountCreateRequest {
            Name = "kv-account",
            ApplicationId = "app-id",
            DirectoryId = "dir-id",
            Environment = AzureEnvironment.AzureUsGovernment,
            ApplicationSecret = "secret"
        };

        var id = await client.CreateAccountAsync(request, CancellationToken.None).ConfigureAwait(false);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Post, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/azure/v1/accounts", handler.ApiRequest.RequestUri!.ToString());

        var body = await handler.ApiRequest.Content!.ReadAsStringAsync().ConfigureAwait(false);
        using var doc = JsonDocument.Parse(body);
        var root = doc.RootElement;
        Assert.Equal("kv-account", root.GetProperty("name").GetString());
        Assert.Equal("app-id", root.GetProperty("applicationId").GetString());
        Assert.Equal("dir-id", root.GetProperty("directoryId").GetString());
        Assert.Equal("AZURE_US_GOVERNMENT", root.GetProperty("environment").GetString());
        Assert.Equal("secret", root.GetProperty("applicationSecret").GetString());

        Assert.Equal(42, id);
    }

    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _tokenResponse;
        private readonly HttpResponseMessage _apiResponse;

        public HttpRequestMessage? TokenRequest { get; private set; }

        public HttpRequestMessage? ApiRequest { get; private set; }

        public TestHandler(HttpResponseMessage tokenResponse, HttpResponseMessage apiResponse) {
            _tokenResponse = tokenResponse ?? throw new ArgumentNullException(nameof(tokenResponse));
            _apiResponse = apiResponse ?? throw new ArgumentNullException(nameof(apiResponse));
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (request.RequestUri!.AbsoluteUri.Contains("protocol/openid-connect/token", StringComparison.OrdinalIgnoreCase)) {
                TokenRequest = request;
                return Task.FromResult(_tokenResponse);
            }

            ApiRequest = request;
            return Task.FromResult(_apiResponse);
        }
    }
}

