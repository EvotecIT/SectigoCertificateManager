namespace SectigoCertificateManager.Utilities;

using System;
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
                Code = -1,
                Description = $"Failed to parse {typeof(T).Name} from response: {ex.Message}"
            });
        }
    }
}
