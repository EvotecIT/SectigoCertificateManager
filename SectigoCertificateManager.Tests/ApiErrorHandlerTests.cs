using SectigoCertificateManager;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Unit tests for <see cref="ApiErrorHandler"/>.
/// </summary>
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

    /// <summary>Handles 401 responses correctly.</summary>
    [Fact]
    public async Task ThrowsAuthenticationException() {
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) {
            Content = JsonContent.Create(new ApiError { Code = ApiErrorCode.UnknownUser, Description = "Unknown user" })
        };

        using var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => client.GetAsync("v1/test"));
        Assert.Equal(ApiErrorCode.UnknownUser, ex.ErrorCode);
    }

    [Fact]
    public async Task ThrowsValidationException() {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
            Content = JsonContent.Create(new ApiError { Code = ApiErrorCode.ErrorWhileDecodingCsr, Description = "Invalid" })
        };

        using var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<ValidationException>(() => client.GetAsync("v1/test"));
        Assert.Equal(ApiErrorCode.ErrorWhileDecodingCsr, ex.ErrorCode);
    }

    [Fact]
    public async Task ThrowsApiExceptionForOtherErrors() {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError) {
            Content = JsonContent.Create(new ApiError { Code = ApiErrorCode.InternalErrorPleaseContactSupportForDetails, Description = "Boom" })
        };

        using var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.GetAsync("v1/test"));
        Assert.Equal(ApiErrorCode.InternalErrorPleaseContactSupportForDetails, ex.ErrorCode);
    }

    [Fact]
    public async Task ThrowsApiException_WhenErrorBodyInvalid() {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
            Content = new StringContent("oops")
        };

        using var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.GetAsync("v1/test"));
        Assert.Equal(ApiErrorCode.UnknownError, ex.ErrorCode);
    }

    [Fact]
    public async Task ExceptionMessageIncludesStatusCodeAndBody() {
        const string body = "{\"code\":1}";
        var response = new HttpResponseMessage(HttpStatusCode.Unauthorized) {
            Content = new StringContent(body)
        };

        using var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<AuthenticationException>(() => client.GetAsync("v1/test"));
        Assert.Contains("StatusCode: 401", ex.Message);
        Assert.Contains(body, ex.Message);
    }

    [Fact]
    public async Task ExceptionMessageTruncatesLongBody() {
        var longBody = new string('a', 250);
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest) {
            Content = new StringContent(longBody)
        };

        using var client = CreateClient(response);

        var ex = await Assert.ThrowsAsync<ApiException>(() => client.GetAsync("v1/test"));
        Assert.Contains(new string('a', 200), ex.Message);
        Assert.DoesNotContain(new string('a', 201), ex.Message);
    }
}