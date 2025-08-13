namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;
using System.Globalization;
using System.Net.Http.Json;
using System.Text;

/// <summary>
/// Provides access to certificate related endpoints.
/// </summary>
public sealed partial class CertificatesClient : BaseClient {
    /// <summary>
    /// Initializes a new instance of the <see cref="CertificatesClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public CertificatesClient(ISectigoClient client) : base(client) {
    }

    /// <summary>
    /// Issues a new certificate.
    /// </summary>
    /// <param name="request">Payload describing the certificate to issue.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Certificate?> IssueAsync(IssueCertificateRequest request, CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        if (request.Term <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.Term));
        }

        var response = await _client.PostAsync("v1/certificate/issue", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<Certificate>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Revokes a certificate.
    /// </summary>
    /// <param name="request">Payload describing the certificate to revoke.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeAsync(RevokeCertificateRequest request, CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var response = await _client.PostAsync("v1/certificate/revoke", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to delete.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteAsync(int certificateId, CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        var response = await _client.DeleteAsync($"v1/certificate/{certificateId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Validates a certificate request without issuing it.
    /// </summary>
    /// <param name="request">Payload describing the certificate to validate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<ValidateCertificateResponse?> ValidateCertificateRequestAsync(
        ValidateCertificateRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        var response = await _client
            .PostAsync("v1/certificate/validate", JsonContent.Create(request, options: s_json), cancellationToken)
            .ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<ValidateCertificateResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    private static string BuildQuery(CertificateSearchRequest request) {
        var builder = new StringBuilder();

        void AppendSeparator() {
            _ = builder.Length == 0 ? builder.Append('?') : builder.Append('&');
        }

        void Append(string name, string? value) {
            if (string.IsNullOrEmpty(value)) {
                return;
            }

            AppendSeparator();
            builder.Append(name).Append('=').Append(Uri.EscapeDataString(value));
        }

        void AppendInt(string name, int value) {
            AppendSeparator();
            builder.Append(name).Append('=').Append(value);
        }

        void AppendDate(string name, DateTime? value) {
            if (!value.HasValue) {
                return;
            }

            AppendSeparator();
            builder.Append(name).Append('=')
                .Append(value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        }

        if (request.Size.HasValue) {
            AppendInt("size", request.Size.Value);
        }

        if (request.Position.HasValue) {
            AppendInt("position", request.Position.Value);
        }

        Append("commonName", request.CommonName);
        Append("subjectAlternativeName", request.SubjectAlternativeName);

        if (request.Status.HasValue && request.Status.Value != CertificateStatus.Any) {
            Append("status", request.Status.Value.ToString());
        }

        if (request.SslTypeId.HasValue) {
            AppendInt("sslTypeId", request.SslTypeId.Value);
        }

        Append("discoveryStatus", request.DiscoveryStatus);
        Append("vendor", request.Vendor);

        if (request.OrgId.HasValue) {
            AppendInt("orgId", request.OrgId.Value);
        }

        Append("installStatus", request.InstallStatus);
        Append("renewalStatus", request.RenewalStatus);
        Append("issuer", request.Issuer);
        Append("issuerDN", request.IssuerDn);
        Append("serialNumber", request.SerialNumber);
        Append("requester", request.Requester);
        Append("externalRequester", request.ExternalRequester);
        Append("signatureAlgorithm", request.SignatureAlgorithm);
        Append("keyAlgorithm", request.KeyAlgorithm);

        if (request.KeySize.HasValue) {
            AppendInt("keySize", request.KeySize.Value);
        }

        Append("keyParam", request.KeyParam);
        Append("sha1Hash", request.Sha1Hash);
        Append("md5Hash", request.Md5Hash);
        Append("keyUsage", request.KeyUsage);
        Append("extendedKeyUsage", request.ExtendedKeyUsage);
        Append("requestedVia", request.RequestedVia);
        AppendDate("dateFrom", request.DateFrom);
        AppendDate("dateTo", request.DateTo);

        return builder.ToString();
    }
}
