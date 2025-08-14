using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="CertificateTypesClient"/>.
/// </summary>
public sealed class CertificateTypesClientTests {
    private sealed class StubClient : ISectigoClient {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }
        public HttpClient HttpClient { get; } = new();
        public bool EnableDownloadCache => true;

        public StubClient(HttpResponseMessage response) => _response = response;

        public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken = default) {
            Request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return Task.FromResult(_response);
        }

        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
            => throw new System.NotImplementedException();

        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken = default)
            => throw new System.NotImplementedException();

        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken = default)
            => throw new System.NotImplementedException();
    }

    /// <summary>Lists certificate types.</summary>
    [Fact]
    public async Task ListTypesAsync_ReturnsTypes() {
        var type = new CertificateType { Id = 1, Name = "SSL" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new[] { type })
        };
        var client = new StubClient(response);
        var typesClient = new CertificateTypesClient(client);

        var result = await typesClient.ListTypesAsync();

        Assert.NotNull(client.Request);
        Assert.Equal("v1/certificate/types", client.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    /// <summary>Appends query when organizationId provided.</summary>
    [Fact]
    public async Task ListTypesAsync_WithOrganizationId_AppendsQuery() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create<object?>(null)
        };
        var client = new StubClient(response);
        var typesClient = new CertificateTypesClient(client);

        await typesClient.ListTypesAsync(5);

        Assert.NotNull(client.Request);
        Assert.Equal("v1/certificate/types?organizationId=5", client.Request!.RequestUri!.ToString());
    }

    /// <summary>Handles null response.</summary>
    [Fact]
    public async Task ListTypesAsync_ReturnsEmpty_WhenResponseNull() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create<object?>(null)
        };
        var client = new StubClient(response);
        var typesClient = new CertificateTypesClient(client);

        var result = await typesClient.ListTypesAsync();

        Assert.NotNull(client.Request);
        Assert.Equal("v1/certificate/types", client.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Empty(result);
    }
}
