namespace SectigoCertificateManager;

        try {
            error = JsonSerializer.Deserialize<ApiError>(body, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        } catch (Exception ex) when (ex is JsonException || ex is NotSupportedException) {

using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;        if (!string.IsNullOrWhiteSpace(error.Description)) {
            message += $", Error: {error.Description}";
        }
        if ((int)error.Code != 0) {
            message += $", Code: {(int)error.Code}";
        }

        error.Description = message;
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
