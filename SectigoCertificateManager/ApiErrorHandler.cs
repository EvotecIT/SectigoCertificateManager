namespace SectigoCertificateManager;

using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Centralized helper that inspects HTTP responses and throws rich exceptions
/// (<see cref="ApiException"/>, <see cref="AuthenticationException"/>, <see cref="ValidationException"/>)
/// when the Sectigo API reports an error.
/// </summary>
internal static class ApiErrorHandler {
    /// <summary>
    /// Throws an appropriate exception when the response is not successful.
    /// The exception message includes status code, a truncated copy of the body,
    /// and any error details returned by the API.
    /// </summary>
    /// <param name="response">HTTP response message to inspect.</param>
    /// <param name="cancellationToken">Token used to cancel the read of the response body.</param>
    public static async Task ThrowIfErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken = default) {
        if (response.IsSuccessStatusCode) {
            return;
        }

        const int maxBodyLength = 200;
#if NET5_0_OR_GREATER
        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#else
        var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#endif
        var snippet = body.Length > maxBodyLength
            ? body.Substring(0, maxBodyLength) + "..."
            : body;

        ApiError? error = null;
        try {
            error = JsonSerializer.Deserialize<ApiError>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        } catch (Exception ex) when (ex is JsonException or NotSupportedException) {
            var parseMessage = $"StatusCode: {(int)response.StatusCode} ({response.StatusCode})";
            if (!string.IsNullOrWhiteSpace(snippet)) {
                parseMessage += $", Body: {snippet}";
            }
            parseMessage += $", Error: Failed to parse ApiError from response: {ex.Message}";
            throw new ApiException(new ApiError {
                Code = ApiErrorCode.UnknownError,
                Description = parseMessage
            });
        }

        error ??= new ApiError {
            Code = (ApiErrorCode)(int)response.StatusCode,
            Description = response.ReasonPhrase ?? "Request failed"
        };

        var message = $"StatusCode: {(int)response.StatusCode} ({response.StatusCode})";
        if (!string.IsNullOrWhiteSpace(snippet)) {
            message += $", Body: {snippet}";
        }

        if (!string.IsNullOrWhiteSpace(error.Description)) {
            message += $", Error: {error.Description}";
        }

        error.Description = message;

        throw response.StatusCode switch {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AuthenticationException(error),
            HttpStatusCode.BadRequest => new ValidationException(error),
            _ => new ApiException(error)
        };
    }
}
