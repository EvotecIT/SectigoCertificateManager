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

/// <summary>
/// Tests for <see cref="CustomFieldsClient"/>.
/// </summary>
public sealed class CustomFieldsClientTests {
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
    public async Task CreateAsync_SendsPayloadAndReturnsField() {
        var field = new CustomField { Id = 3, Name = "field" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(field)
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var fields = new CustomFieldsClient(client);

        var request = new CreateCustomFieldRequest { Name = "field", Mandatory = true, CertType = "ssl", State = "ACTIVE" };
        var result = await fields.CreateAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/customfield", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"name\":\"field\"", handler.Body);
        Assert.NotNull(result);
        Assert.Equal(3, result!.Id);
    }

    [Fact]
    public async Task UpdateAsync_SendsPayload() {
        var field = new CustomField { Id = 5, Name = "new" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(field)
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var fields = new CustomFieldsClient(client);

        var request = new UpdateCustomFieldRequest { Id = 5, Name = "new", Mandatory = false, CertType = "ssl", State = "ACTIVE" };
        var result = await fields.UpdateAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Put, handler.Request!.Method);
        Assert.Equal("https://example.com/v1/customfield", handler.Request.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"id\":5", handler.Body);
        Assert.NotNull(result);
        Assert.Equal("new", result!.Name);
    }

    [Fact]
    public async Task DeleteAsync_SendsDeleteRequest() {
        var response = new HttpResponseMessage(HttpStatusCode.NoContent);
        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var fields = new CustomFieldsClient(client);

        await fields.DeleteAsync(7);

        Assert.NotNull(handler.Request);
        Assert.Equal(HttpMethod.Delete, handler.Request!.Method);
        Assert.Equal("https://example.com/v1/customfield/7", handler.Request.RequestUri!.ToString());
    }
}
