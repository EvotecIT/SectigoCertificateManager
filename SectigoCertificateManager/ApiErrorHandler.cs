namespace SectigoCertificateManager;

using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

internal static class ApiErrorHandler
{
    public static async Task ThrowIfErrorAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        ApiError? error = null;
        try
        {
            error = await response.Content.ReadFromJsonAsync<ApiError>().ConfigureAwait(false);
        }
        catch
        {
            // ignore parsing errors
        }

        error ??= new ApiError
        {
            Code = (int)response.StatusCode,
            Description = response.ReasonPhrase ?? "Request failed",
        };

        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AuthenticationException(error),
            HttpStatusCode.BadRequest => new ValidationException(error),
            _ => new ApiException(error),
        };
    }
}
