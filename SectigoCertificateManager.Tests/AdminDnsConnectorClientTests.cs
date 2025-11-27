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

public sealed class AdminDnsConnectorClientTests {
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
            Content = new StringContent("{\"access_token\":\"xyz\",\"expires_in\":300}")
        };

        var connectorsPayload =
            "[{\"name\":\"dns-1\",\"comments\":\"test\",\"id\":\"abc\",\"version\":\"1.0\",\"revision\":\"r1\",\"hostname\":\"host\",\"os\":\"linux\",\"status\":\"CONNECTED\",\"delegationMode\":\"GLOBAL_FOR_CUSTOMER\"}]";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(connectorsPayload, Encoding.UTF8, "application/json")
        };
        apiResponse.Headers.Add("X-Total-Count", "3");

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminDnsConnectorClient(CreateConfig(), httpClient);
        var (items, total) = await client.ListAsync(10, 5, "dns-1", DnsConnectorStatus.Connected, new[] { 100, 200 }, CancellationToken.None);

        Assert.Equal("https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token", handler.TokenRequest?.RequestUri?.ToString());

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/connector/v1/dns?size=10&position=5&name=dns-1&status=CONNECTED&orgIds=100&orgIds=200",
            handler.ApiRequest.RequestUri!.ToString());

        Assert.Single(items);
        var first = items[0];
        Assert.Equal("dns-1", first.Name);
        Assert.Equal("abc", first.Id);
        Assert.Equal(DnsConnectorStatus.Connected, first.Status);
        Assert.Equal(DnsConnectorDelegationMode.GlobalForCustomer, first.DelegationMode);
        Assert.Equal(3, total);
    }

    [Fact]
    public async Task GetAsync_BuildsUriAndParsesDetails() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"xyz\",\"expires_in\":300}")
        };

        var detailsPayload =
            "{\"name\":\"dns-1\",\"comments\":\"test\",\"id\":\"abc\",\"version\":\"1.0\",\"revision\":\"r1\",\"hostname\":\"host\",\"os\":\"linux\",\"status\":\"NOT_CONNECTED\",\"delegationMode\":\"CUSTOMIZED\",\"delegatedOrganizations\":[{\"id\":1,\"name\":\"Org1\"}]}";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(detailsPayload, Encoding.UTF8, "application/json")
        };

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminDnsConnectorClient(CreateConfig(), httpClient);
        var result = await client.GetAsync("abc-uuid", CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/connector/v1/dns/abc-uuid", handler.ApiRequest.RequestUri!.ToString());

        Assert.NotNull(result);
        Assert.Equal("dns-1", result!.Name);
        Assert.Equal(DnsConnectorStatus.NotConnected, result.Status);
        Assert.Equal(DnsConnectorDelegationMode.Customized, result.DelegationMode);
        Assert.Single(result.DelegatedOrganizations);
        Assert.Equal(1, result.DelegatedOrganizations[0].Id);
        Assert.Equal("Org1", result.DelegatedOrganizations[0].Name);
    }

    [Fact]
    public async Task ListProvidersAsync_BuildsUriAndParsesResponse() {
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent("{\"access_token\":\"xyz\",\"expires_in\":300}")
        };

        var providersPayload = "[\"ROUTE_53\",\"CLOUDFLARE\"]";
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = new StringContent(providersPayload, Encoding.UTF8, "application/json")
        };
        apiResponse.Headers.Add("X-Total-Count", "2");

        using var handler = new TestHandler(tokenResponse, apiResponse);
        using var httpClient = new HttpClient(handler) {
            BaseAddress = new Uri("https://admin.enterprise.sectigo.com/")
        };

        var client = new AdminDnsConnectorClient(CreateConfig(), httpClient);
        var (providers, total) = await client.ListProvidersAsync("abc-uuid", CancellationToken.None);

        Assert.NotNull(handler.ApiRequest);
        Assert.Equal(HttpMethod.Get, handler.ApiRequest!.Method);
        Assert.Equal("https://admin.enterprise.sectigo.com/api/connector/v1/dns/abc-uuid/provider", handler.ApiRequest.RequestUri!.ToString());

        Assert.Equal(2, providers.Count);
        Assert.Equal("ROUTE_53", providers[0]);
        Assert.Equal("CLOUDFLARE", providers[1]);
        Assert.Equal(2, total);
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

