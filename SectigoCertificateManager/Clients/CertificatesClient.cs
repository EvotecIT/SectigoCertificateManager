namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.Net.Http.Json;

/// <summary>
/// Provides access to certificate related endpoints.
/// </summary>
public sealed class CertificatesClient {
    private readonly ISectigoClient _client;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificatesClient"/> class.
    /// </summary>
    /// <param name="client">HTTP client wrapper.</param>
    public CertificatesClient(ISectigoClient client) => _client = client;

    /// <summary>
    /// Retrieves a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Certificate?> GetAsync(int certificateId, CancellationToken cancellationToken = default) {
        var response = await _client.GetAsync($"v1/certificate/{certificateId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Certificate>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Issues a new certificate.
    /// </summary>
    /// <param name="request">Payload describing the certificate to issue.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Certificate?> IssueAsync(IssueCertificateRequest request, CancellationToken cancellationToken = default) {
        var response = await _client.PostAsync("v1/certificate/issue", JsonContent.Create(request), cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<Certificate>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Searches for certificates using the provided filter.
    /// </summary>
    public async Task<CertificateResponse?> SearchAsync(CertificateSearchRequest request, CancellationToken cancellationToken = default) {
        var query = BuildQuery(request);
        var response = await _client.GetAsync($"v1/certificate{query}", cancellationToken);
        response.EnsureSuccessStatusCode();
        var items = await response.Content.ReadFromJsonAsync<IReadOnlyList<Certificate>>(cancellationToken: cancellationToken);
        return items is null ? null : new CertificateResponse { Certificates = items };
    }

    private static string BuildQuery(CertificateSearchRequest request) {
        var parts = new List<string>();

        if (request.Size.HasValue) {
            parts.Add($"size={request.Size.Value}");
        }

        if (request.Position.HasValue) {
            parts.Add($"position={request.Position.Value}");
        }

        Add(parts, "commonName", request.CommonName);
        Add(parts, "subjectAlternativeName", request.SubjectAlternativeName);

        if (request.Status.HasValue && request.Status.Value != CertificateStatus.Any) {
            Add(parts, "status", request.Status.Value.ToString());
        }

        if (request.SslTypeId.HasValue) {
            parts.Add($"sslTypeId={request.SslTypeId.Value}");
        }

        Add(parts, "discoveryStatus", request.DiscoveryStatus);
        Add(parts, "vendor", request.Vendor);

        if (request.OrgId.HasValue) {
            parts.Add($"orgId={request.OrgId.Value}");
        }

        Add(parts, "installStatus", request.InstallStatus);
        Add(parts, "renewalStatus", request.RenewalStatus);
        Add(parts, "issuer", request.Issuer);
        Add(parts, "serialNumber", request.SerialNumber);
        Add(parts, "requester", request.Requester);
        Add(parts, "externalRequester", request.ExternalRequester);
        Add(parts, "signatureAlgorithm", request.SignatureAlgorithm);
        Add(parts, "keyAlgorithm", request.KeyAlgorithm);

        if (request.KeySize.HasValue) {
            parts.Add($"keySize={request.KeySize.Value}");
        }

        Add(parts, "keyParam", request.KeyParam);
        Add(parts, "sha1Hash", request.Sha1Hash);
        Add(parts, "md5Hash", request.Md5Hash);
        Add(parts, "keyUsage", request.KeyUsage);
        Add(parts, "extendedKeyUsage", request.ExtendedKeyUsage);
        Add(parts, "requestedVia", request.RequestedVia);

        return parts.Count > 0 ? "?" + string.Join("&", parts) : string.Empty;
    }

    private static void Add(ICollection<string> parts, string name, string? value) {
        if (!string.IsNullOrEmpty(value)) {
            parts.Add($"{name}={Uri.EscapeDataString(value)}");
        }
    }
}