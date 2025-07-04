using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class ProfilesClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            return Task.FromResult(_response);
        }
    }

    [Fact]
    public async Task ListProfilesAsync_RequestsProfiles() {
        var profile = new Profile { Id = 2, Name = "SSL" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new[] { profile })
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var profiles = new ProfilesClient(client);

        var result = await profiles.ListProfilesAsync();

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/profile", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Single(result!);
        Assert.Equal(2, result[0].Id);
    }
}
