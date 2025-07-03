namespace SectigoCertificateManager;

using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

public static class HttpResponseMessageExtensions
{
    public static async Task EnsureSuccessWithApiErrorAsync(this HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        ApiError? error = null;
        try
        {
            error = JsonSerializer.Deserialize<ApiError>(content);
        }
        catch (JsonException)
        {
        }

        var message = error?.Description ?? error?.Message ?? error?.Error ?? content;

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new ApiAuthenticationException(message, response.StatusCode),
            HttpStatusCode.BadRequest => new ApiValidationException(message, response.StatusCode),
            _ => new ApiException(message, response.StatusCode),
        };
    }
}
