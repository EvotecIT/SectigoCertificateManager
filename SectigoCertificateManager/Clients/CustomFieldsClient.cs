namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Provides access to custom field endpoints.
/// </summary>
public sealed class CustomFieldsClient : BaseClient {

    /// <summary>
    /// Initializes a new instance of the <see cref="CustomFieldsClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public CustomFieldsClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Creates a new custom field.
    /// </summary>
    /// <param name="request">Payload describing the field to create.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<CustomField?> CreateAsync(
        CreateCustomFieldRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var response = await _client.PostAsync(
            "v1/customfield",
            JsonContent.Create(request, options: s_json),
            cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<CustomField>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Updates an existing custom field.
    /// </summary>
    /// <param name="request">Payload describing updated field properties.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<CustomField?> UpdateAsync(
        UpdateCustomFieldRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (request.Id <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.Id));
        }

        var response = await _client.PutAsync(
            "v1/customfield",
            JsonContent.Create(request, options: s_json),
            cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<CustomField>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a custom field by identifier.
    /// </summary>
    /// <param name="customFieldId">Identifier of the custom field to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteAsync(int customFieldId, CancellationToken cancellationToken = default) {
        if (customFieldId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(customFieldId));
        }

        var response = await _client.DeleteAsync(
            $"v1/customfield/{customFieldId}",
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}
