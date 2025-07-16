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
/// Unit tests for <see cref="UsersClient"/>.
/// </summary>
public sealed class UsersClientTests {
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
    public async Task ListUsersAsync_RequestsUsers() {
        var user = new User { Id = 1, Email = "a" };
        var response = new HttpResponseMessage(HttpStatusCode.OK) {
            Content = JsonContent.Create(new[] { user })
        };

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var users = new UsersClient(client);

        var result = await users.ListUsersAsync();

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/user?size=200&position=0", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result[0].Id);
    }

    [Fact]
    public async Task CreateAsync_SendsPayloadAndReturnsId() {
        var response = new HttpResponseMessage(HttpStatusCode.Created);
        response.Headers.Location = new System.Uri("https://example.com/v1/user/5");

        var handler = new TestHandler(response);
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var users = new UsersClient(client);

        var request = new CreateUserRequest { Email = "a", ValidationType = "STANDARD", OrganizationId = 1 };
        var id = await users.CreateAsync(request);

        Assert.NotNull(handler.Request);
        Assert.Equal("https://example.com/v1/user", handler.Request!.RequestUri!.ToString());
        Assert.NotNull(handler.Body);
        Assert.Contains("\"email\":\"a\"", handler.Body);
        Assert.Equal(5, id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetAsync_InvalidUserId_Throws(int userId) {
        var handler = new TestHandler(new HttpResponseMessage(HttpStatusCode.OK));
        using var httpClient = new HttpClient(handler);
        var client = new SectigoClient(new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4), httpClient);
        var users = new UsersClient(client);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => users.GetAsync(userId));
    }
}
