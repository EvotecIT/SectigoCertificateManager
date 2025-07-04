using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
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
        public HttpMethod? Method { get; private set; }
        public Uri? Uri { get; private set; }
        public string? Content { get; private set; }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Method = request.Method;
            Uri = request.RequestUri;

            if (request.Content is not null)
            {
                Content = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            return new HttpResponseMessage(HttpStatusCode.NoContent);
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

        Assert.Equal(HttpMethod.Post, handler.Method);
        Assert.Equal(new Uri("https://example.com/api/v1/revoke/10"), handler.Uri);
        Assert.Equal("{\"reason\":\"foo\"}", handler.Content);
    }
}
