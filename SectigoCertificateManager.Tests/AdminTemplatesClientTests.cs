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

/// <summary>
/// Tests for <see cref="AdminTemplatesClient"/>.
/// </summary>
public sealed class AdminTemplatesClientTests {
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
    public async Task GetAsync_ReturnsTemplate() {
        var template = new IdpTemplate { Id = 5, Name = "t" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(template) };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var templates = new AdminTemplatesClient(client);

        var result = await templates.GetAsync(5);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/admin-template/v1/5", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
    }

    [Fact]
    public async Task CreateAsync_ReturnsId() {
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Headers.Location = new System.Uri("https://example.com/admin-template/v1/7");

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var templates = new AdminTemplatesClient(client);

        var id = await templates.CreateAsync(new CreateIdpTemplateRequest { Name = "t" });

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/admin-template/v1/", handler.Request!.RequestUri!.ToString());
        Assert.Equal(7, id);
    }

    [Fact]
    public async Task UpdateAsync_SendsPayload() {
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var templates = new AdminTemplatesClient(client);

        var request = new UpdateIdpTemplateRequest { Name = "n" };
        await templates.UpdateAsync(9, request);

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Put, handler.Request!.Method);
        Assert.Equal("https://example.com/admin-template/v1/9", handler.Request.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"name\":\"n\"", handler.Body);
    }

    [Fact]
    public async Task DeleteAsync_SendsRequest() {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var templates = new AdminTemplatesClient(client);

        await templates.DeleteAsync(4, RelatedAdminsAction.Delete, 3);

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Delete, handler.Request!.Method);
        Assert.Equal("https://example.com/admin-template/v1/4?relatedAdminsAction=Delete&replacingRequesterId=3", handler.Request.RequestUri!.ToString());
    }
}
