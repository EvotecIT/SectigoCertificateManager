using SectigoCertificateManager;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class ApiErrorHandlerTests {
    private sealed class RespondingHandler : HttpMessageHandler {
        private readonly HttpResponseMessage _response;

        public RespondingHandler(HttpResponseMessage response) {
            _response = response;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            => Task.FromResult(_response);
    }

    private static SectigoClient CreateClient(HttpResponseMessage response) {
        var config = new ApiConfig("https://example.com/", "u", "p", "c", ApiVersion.V25_4);
        var handler = new RespondingHandler(response);
        return new SectigoClient(config, new HttpClient(handler));
    }

    [Fact]
    public async Task ThrowsAuthenticationException() {
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) {
            Content = JsonContent.Create(new ApiError { Code = -16, Description = "Unknown user" })
        };

        var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => client.GetAsync("v1/test"));
        Assert.Equal(-16, ex.ErrorCode);
    }

    [Fact]
    public async Task ThrowsValidationException() {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
            Content = JsonContent.Create(new ApiError { Code = -10, Description = "Invalid" })
        };

        var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => client.GetAsync("v1/test"));
        Assert.Equal(-10, ex.ErrorCode);
    }

    [Fact]
    public async Task ThrowsApiExceptionForOtherErrors() {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) {
            Content = JsonContent.Create(new ApiError { Code = -2, Description = "Boom" })
        };

        var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.GetAsync("v1/test"));
        Assert.Equal(-2, ex.ErrorCode);
    }
}