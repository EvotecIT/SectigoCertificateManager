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
/// Tests for <see cref="ProfilesClient"/>.
/// </summary>
public sealed class ProfilesClientTests {
    private sealed class StubClient : ISectigoClient {
        private readonly HttpResponseMessage _response;
        public HttpRequestMessage? Request { get; private set; }
        public HttpClient HttpClient { get; } = new();

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

    /// <summary>Gets a profile by identifier.</summary>
    [Fact]
    public async Task GetAsync_ReturnsProfile() {
        var profile = new Profile {
            Id = 1,
            Name = "Test",
            Description = "Profile",
            Terms = new[] { 12, 24 },
            KeyTypes = new Dictionary<string, IReadOnlyList<string>> { ["RSA"] = new[] { "2048" } },
            UseSecondaryOrgName = true
        };
        var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = JsonContent.Create(profile) };
        var client = new StubClient(response);
        var profiles = new ProfilesClient(client);

        var result = await profiles.GetAsync(1);

        Assert.NotNull(client.Request);
        Assert.Equal("v1/profile/1", client.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("Test", result.Name);
        Assert.Equal("Profile", result.Description);
        Assert.Equal(2, result.Terms.Count);
        Assert.True(result.UseSecondaryOrgName);
        Assert.True(result.KeyTypes.ContainsKey("RSA"));
        Assert.Equal("2048", result.KeyTypes["RSA"][0]);
    }

    [Fact]
    public async Task ListProfilesAsync_ReturnsProfiles() {
        var profile = new Profile { Id = 2, Name = "Test" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new[] { profile })
        };
        var client = new StubClient(response);
        var profiles = new ProfilesClient(client);

        var result = await profiles.ListProfilesAsync();

        Assert.NotNull(client.Request);
        Assert.Equal("v1/profile", client.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(2, result[0].Id);
    }

    [Fact]
    public async Task ListProfilesAsync_ReturnsEmpty_WhenResponseNull() {
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create<object?>(null)
        };
        var client = new StubClient(response);
        var profiles = new ProfilesClient(client);

        var result = await profiles.ListProfilesAsync();

        Assert.NotNull(client.Request);
        Assert.Equal("v1/profile", client.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAsync_DisposesResponse() {
        var response = new DisposableResponse {
            StatusCode = HttpStatusCode.OK,
            Content = JsonContent.Create(new Profile())
        };
        var client = new StubClient(response);
        var profiles = new ProfilesClient(client);

        _ = await profiles.GetAsync(1);

        Assert.True(response.Disposed);
    }
}