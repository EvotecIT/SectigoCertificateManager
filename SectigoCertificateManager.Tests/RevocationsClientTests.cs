using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class RevocationsClientTests
{
    private sealed class TestHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent));
        }
    }

    [Fact]
    public async Task RevokeAsync_SendsRequest()
    {
        var config = new ApiConfig("https://example.com/api/", "user", "pass", "cst1", ApiVersion.V25_4);
        var handler = new TestHandler();
        var httpClient = new HttpClient(handler);
        var client = new SectigoClient(config, httpClient);
        var revocations = new RevocationsClient(client);

        await revocations.RevokeAsync(10, "foo");

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal(new Uri("https://example.com/api/v1/revoke/10"), handler.Request.RequestUri);
        var json = await handler.Request.Content!.ReadAsStringAsync();
        Assert.Equal("{\"reason\":\"foo\"}", json);
    }
}
