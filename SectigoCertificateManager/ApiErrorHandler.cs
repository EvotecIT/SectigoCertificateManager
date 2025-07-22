namespace SectigoCertificateManager;

using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Handles API error responses.
/// </summary>
internal static class ApiErrorHandler {
    /// <summary>
    /// Throws an exception if the response indicates an error.
    /// </summary>
    /// <param name="response">HTTP response message.</param>
    public static async Task ThrowIfErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken = default) {
        if (response.IsSuccessStatusCode) {
            return;
        }

        ApiError? error = null;
        try {
            error = await response.Content
                .ReadFromJsonAsync<ApiError>(cancellationToken)
                .ConfigureAwait(false);
        } catch (Exception ex) when (ex is JsonException or NotSupportedException) {
            throw new ApiException(new ApiError {
                Code = ApiErrorCode.UnknownError,
                Description = $"Failed to parse ApiError from response: {ex.Message}"
            });
        }

        error ??= new ApiError {
            Code = (ApiErrorCode)(int)response.StatusCode,
            Description = response.ReasonPhrase ?? "Request failed",
        };

        throw response.StatusCode switch {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AuthenticationException(error),
            HttpStatusCode.BadRequest => new ValidationException(error),
            _ => new ApiException(error),
        };
    }
}