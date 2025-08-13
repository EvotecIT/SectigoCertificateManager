namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;
using System.Net.Http.Json;

public sealed partial class CertificatesClient : BaseClient {
    /// <summary>
    /// Retrieves a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Certificate?> GetAsync(int certificateId, CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        var response = await _client.GetAsync($"v1/certificate/{certificateId}", cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<Certificate>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the status of a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<CertificateStatus?> GetStatusAsync(int certificateId, CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        var response = await _client.GetAsync($"v1/certificate/{certificateId}/status", cancellationToken).ConfigureAwait(false);
        var result = await response.Content
            .ReadFromJsonAsyncSafe<StatusResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return result?.Status;
    }

    /// <summary>
    /// Retrieves revocation details for a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<CertificateRevocation?> GetRevocationAsync(int certificateId, CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        var response = await _client.GetAsync($"v1/certificate/{certificateId}/revocation", cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<CertificateRevocation>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    private sealed class StatusResponse {
        /// <summary>Gets or sets the certificate status.</summary>
        public CertificateStatus Status { get; set; }
    }
}
