using SectigoCertificateManager.AdminApi;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class AdminAcmeServerClientTests {
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

            var apiResponse = new HttpResponseMessage(_apiStatus);
            if (!string.IsNullOrEmpty(_apiJson)) {
                apiResponse.Content = new StringContent(_apiJson, System.Text.Encoding.UTF8, "application/json");
            }
            return Task.FromResult(apiResponse);
        }
    }

    private static AdminApiConfig CreateConfig() =>
        new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "id",
            "secret");

    [Fact]
    public async Task ListServersAsync_BuildsQueryAndParsesResponse() {
        var token = new { access_token = "tok" };
        var tokenResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(token)
        };

        var servers = new[] {
            new AcmeServerInfo {
                Url = "https://acme.example.com",
                CaId = 40485,
                Name = "OV ACME Server",
                SingleProductId = 1,
                MultiProductId = 2,
                WcProductId = 3,
                CertValidationType = "OV"
            }
        };
        var apiResponse = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(servers)
        };

        var handler = new TestHandler(tokenResponse, apiResponse);
        using var http = new HttpClient(handler);
        var config = CreateConfig();
        var client = new AdminAcmePublicClient(config, http);

        var result = await client.ListServersAsync(
            size: 10,
            position: 0,
            caId: 40485,
            certValidationType: "OV",
            url: "https://acme.example.com",
            name: "OV ACME Server",
            cancellationToken: CancellationToken.None);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(
            "https://admin.enterprise.sectigo.com/api/acme/v1/server?size=10&position=0&caId=40485&certValidationType=OV&url=https%3A%2F%2Facme.example.com&name=OV%20ACME%20Server",
            handler.LastRequest!.RequestUri!.ToString());
        Assert.Single(result);
        Assert.Equal("https://acme.example.com", result[0].Url);
        Assert.Equal(40485, result[0].CaId);
        Assert.Equal("OV ACME Server", result[0].Name);
        Assert.Equal("OV", result[0].CertValidationType);
    }
}

