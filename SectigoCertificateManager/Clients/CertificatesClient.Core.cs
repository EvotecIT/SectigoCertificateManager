namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

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

    /// <summary>
    /// Imports certificates using a zip archive.
    /// </summary>
    /// <param name="orgId">Identifier of the organization.</param>
    /// <param name="stream">Zip archive containing certificates.</param>
    /// <param name="fileName">File name to use for the upload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<ImportCertificateResponse?> ImportAsync(
        int orgId,
        Stream stream,
        string fileName,
        CancellationToken cancellationToken = default) {
        if (orgId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(orgId));
        }
        Guard.AgainstNull(stream, nameof(stream));
        Guard.AgainstNullOrEmpty(fileName, nameof(fileName), "File name cannot be null or empty.");

        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", fileName);

        var response = await _client.PostAsync($"v1/certificate/import?orgId={orgId}", content, cancellationToken).ConfigureAwait(false);
        return await response.Content
            .ReadFromJsonAsyncSafe<ImportCertificateResponse>(s_json, cancellationToken)
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

    private sealed class StatusResponse {
        /// <summary>Gets or sets the certificate status.</summary>
        public CertificateStatus Status { get; set; }
    }
}
