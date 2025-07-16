namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using System.Net.Http.Json;
using System.Text.Json;

/// <summary>
/// Provides access to certificate type information.
/// </summary>
public sealed class CertificateTypesClient {
    private readonly ISectigoClient _client;
    private static readonly JsonSerializerOptions s_json = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateTypesClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public CertificateTypesClient(ISectigoClient client) => _client = client;

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
            .ReadFromJsonAsync<IReadOnlyList<CertificateType>>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return types ?? Array.Empty<CertificateType>();
    }
}
