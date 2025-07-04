using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class CertificatesClientTests
{
    private sealed class RecordingHandler : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }
        private readonly HttpResponseMessage _response;

        public RecordingHandler(HttpResponseMessage response) => _response = response;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Request = request;
            return Task.FromResult(_response);
        }
    }

    private static CertificatesClient CreateClient(HttpResponseMessage response, out RecordingHandler handler)
    {
        handler = new RecordingHandler(response);
        var config = new ApiConfig("https://example.com/api/", "u", "p", "c", ApiVersion.V25_4);
        var client = new SectigoClient(config, new HttpClient(handler));
        return new CertificatesClient(client);
    }

    [Fact]
    public async Task UpdateAsync_SendsRequest()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new Certificate())
        };
        var api = CreateClient(response, out var handler);

        var req = new UpdateCertificateRequest { Comments = "c" };
        await api.UpdateAsync(5, req);

        Assert.Equal(HttpMethod.Put, handler.Request!.Method);
        Assert.Equal(new Uri("https://example.com/api/v1/certificate"), handler.Request.RequestUri);
        var json = await handler.Request.Content!.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        Assert.Equal(5, doc.RootElement.GetProperty("sslId").GetInt32());
        Assert.Equal("c", doc.RootElement.GetProperty("comments").GetString());
    }

    [Fact]
    public async Task UpdateAsync_ThrowsOnError()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = JsonContent.Create(new ApiError { Code = -1, Description = "bad" })
        };
        var api = CreateClient(response, out _);

        await Assert.ThrowsAsync<ValidationException>(() => api.UpdateAsync(1, new UpdateCertificateRequest()));
    }
}
