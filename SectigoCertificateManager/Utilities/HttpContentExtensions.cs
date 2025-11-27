namespace SectigoCertificateManager.Utilities;

using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for <see cref="HttpContent"/>.
/// </summary>
public static class HttpContentExtensions {
    public static async Task<T?> ReadFromJsonAsyncSafe<T>(
        this HttpContent content,
        JsonSerializerOptions options,
        CancellationToken cancellationToken = default) {
        try {
            return await content
                .ReadFromJsonAsync<T>(options, cancellationToken)
                .ConfigureAwait(false);
        } catch (Exception ex) when (ex is JsonException or NotSupportedException) {
            throw new ApiException(new ApiError {
                Code = ApiErrorCode.UnknownError,
                Description = $"Failed to parse {typeof(T).Name} from response: {ex.Message}"
            });
        }
    }

    /// <summary>
    /// Copies the HTTP content to a new <see cref="MemoryStream"/> and returns it,
    /// ensuring the buffer is disposed if the copy fails.
    /// </summary>
    public static async Task<MemoryStream> CopyToMemoryStreamAsync(
        this HttpContent content,
        CancellationToken cancellationToken = default) {
        var buffer = new MemoryStream();
        try {
#if NETSTANDARD2_0 || NET472
            await content.CopyToAsync(buffer).ConfigureAwait(false);
#else
            await content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
#endif
            buffer.Position = 0;
            return buffer;
        } catch {
            buffer.Dispose();
            throw;
        }
    }
}
