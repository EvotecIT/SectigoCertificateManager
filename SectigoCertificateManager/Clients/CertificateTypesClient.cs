namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Provides access to certificate type information.
/// </summary>
public sealed class CertificateTypesClient : BaseClient {

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateTypesClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public CertificateTypesClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Retrieves available certificate types.
    /// </summary>
    /// <param name="organizationId">Optional organization identifier to filter types.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<CertificateType>> ListTypesAsync(
        int? organizationId = null,
        CancellationToken cancellationToken = default) {
        var endpoint = "v1/certificate/types";
        if (organizationId.HasValue) {
            endpoint += $"?organizationId={organizationId.Value}";
        }

        var response = await _client.GetAsync(endpoint, cancellationToken).ConfigureAwait(false);
        var types = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<CertificateType>>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return types ?? Array.Empty<CertificateType>();
    }

    /// <summary>
    /// Creates or updates a certificate type.
    /// </summary>
    /// <param name="type">Certificate type information.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<CertificateType?> UpsertAsync(
        CertificateType type,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(type, nameof(type));

        var content = JsonContent.Create(type, options: s_json);
        HttpResponseMessage response;
        if (type.Id <= 0) {
            response = await _client.PostAsync("v1/certificate/type", content, cancellationToken).ConfigureAwait(false);
        } else {
            response = await _client.PutAsync($"v1/certificate/type/{type.Id}", content, cancellationToken).ConfigureAwait(false);
        }

        return await response.Content
            .ReadFromJsonAsyncSafe<CertificateType>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }
}
