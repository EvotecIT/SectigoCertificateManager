namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;
using System.Net.Http.Json;

public sealed partial class CertificatesClient : BaseClient {
    /// <summary>
    /// Renews a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to renew.</param>
    /// <param name="request">Payload describing renewal parameters.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the newly issued certificate.</returns>
    public async Task<int> RenewAsync(int certificateId, RenewCertificateRequest request, CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var response = await _client.PostAsync($"v1/certificate/renewById/{certificateId}", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        var result = await response.Content
            .ReadFromJsonAsyncSafe<RenewCertificateResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return result?.SslId ?? 0;
    }

    /// <summary>
    /// Renews a certificate by order number.
    /// </summary>
    /// <param name="orderNumber">Order number used to identify the certificate.</param>
    /// <param name="request">Payload describing renewal parameters.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the newly issued certificate.</returns>
    public async Task<int> RenewByOrderNumberAsync(long orderNumber, RenewCertificateRequest request, CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var response = await _client.PostAsync($"v1/certificate/renew/{orderNumber}", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        var result = await response.Content
            .ReadFromJsonAsyncSafe<RenewCertificateResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return result?.SslId ?? 0;
    }
}
