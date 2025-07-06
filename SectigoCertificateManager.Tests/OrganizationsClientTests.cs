using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class OrganizationsClientTests {
    private sealed class TestHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }
        public string? Body { get; private set; }

        public TestHandler(HttpResponseMessage response) => _response = response;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            Request = request;
            if (request.Content is not null) {
                Body = await request.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
            return _response;
        }
    }

    [Fact]
    public async Task GetAsync_ReturnsOrganization() {
        var org = new Organization { Id = 3, Name = "org" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(org)
        };

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var organizations = new OrganizationsClient(client);

        var result = await organizations.GetAsync(3);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/organization/3", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(3, result!.Id);
    }

    [Fact]
    public async Task CreateAsync_SendsPayloadAndReturnsId() {
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Headers.Location = new System.Uri("https://example.com/v1/organization/10");

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var organizations = new OrganizationsClient(client);

        var request = new CreateOrganizationRequest { Name = "org" };
        var id = await organizations.CreateAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/organization", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"name\":\"org\"", handler.Body);
        Assert.Equal(10, id);
    }

    [Fact]
    public async Task CreateAsync_ParsesId_WhenLocationEndsWithSlash() {
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Headers.Location = new System.Uri("https://example.com/v1/organization/11/");

        var handler = new TestHandler(response);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var organizations = new OrganizationsClient(client);

        var request = new CreateOrganizationRequest { Name = "org" };
        var id = await organizations.CreateAsync(request);

        Assert.Equal(11, id);
    }

    [Fact]
    public async Task CreateAsync_NullRequest_Throws() {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.Created));
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), new HttpClient(handler));
        var organizations = new OrganizationsClient(client);

        await Assert.ThrowsAsync<ArgumentNullException>(() => organizations.CreateAsync(null!));
    }
}