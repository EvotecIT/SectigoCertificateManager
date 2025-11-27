namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager;
using SectigoCertificateManager.Utilities;
using SectigoCertificateManager.Requests;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Client for the Admin Operations API global custom fields (<c>/api/customField/v2</c>) endpoints.
/// </summary>
public sealed class AdminCustomFieldsV2Client : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCustomFieldsV2Client"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminCustomFieldsV2Client(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists custom fields, optionally filtered by certificate type.
    /// </summary>
    /// <param name="certType">
    /// Optional certificate type filter (for example, <c>SSL</c>, <c>SMIME</c>, <c>Device</c>, <c>CodeSign</c>).
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminCustomFieldV2>> ListAsync(
        string? certType = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = new StringBuilder("api/customField/v2");
        if (!string.IsNullOrWhiteSpace(certType)) {
            path.Append("?certType=").Append(Uri.EscapeDataString(certType));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var fields = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminCustomFieldV2>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return fields ?? Array.Empty<AdminCustomFieldV2>();
    }

    /// <summary>
    /// Retrieves a single custom field by identifier.
    /// </summary>
    /// <param name="id">Custom field identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminCustomFieldV2?> GetAsync(
        int id,
        CancellationToken cancellationToken = default) {
        if (id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(id));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/customField/v2/{id}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<AdminCustomFieldV2>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a new custom field.
    /// </summary>
    /// <param name="request">Creation request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created custom field, or 0 when the API does not return a location header.</returns>
    public async Task<int> CreateAsync(
        CreateCustomFieldV2Request request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/customField/v2") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return LocationHeaderParser.ParseId(response);
    }

    /// <summary>
    /// Updates an existing custom field.
    /// </summary>
    /// <param name="field">Updated field definition.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminCustomFieldV2> UpdateAsync(
        AdminCustomFieldV2 field,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(field, nameof(field));
        if (field.Id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(field.Id));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Put, "api/customField/v2") {
            Content = JsonContent.Create(field, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var updated = await response.Content
            .ReadFromJsonAsyncSafe<AdminCustomFieldV2>(s_json, cancellationToken)
            .ConfigureAwait(false);

        if (updated is null) {
            throw new InvalidOperationException("Custom field update response was empty.");
        }

        return updated;
    }

}
