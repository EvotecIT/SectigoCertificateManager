namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using System.IO;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;

/// <summary>
/// Provides access to certificate related endpoints.
/// </summary>
public sealed class CertificatesClient {
    private readonly ISectigoClient _client;
    private static readonly JsonSerializerOptions s_json = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

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
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        using var response = await _client.GetAsync($"v1/certificate/{certificateId}", cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<Certificate>(s_json, cancellationToken).ConfigureAwait(false);
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

        using var response = await _client.GetAsync($"v1/certificate/{certificateId}/status", cancellationToken).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<StatusResponse>(s_json, cancellationToken).ConfigureAwait(false);
        return result?.Status;
    }

    /// <summary>
    /// Issues a new certificate.
    /// </summary>
    /// <param name="request">Payload describing the certificate to issue.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Certificate?> IssueAsync(IssueCertificateRequest request, CancellationToken cancellationToken = default) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        if (request.Term <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.Term));
        }

        using var response = await _client.PostAsync("v1/certificate/issue", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        return await response.Content.ReadFromJsonAsync<Certificate>(s_json, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Revokes a certificate.
    /// </summary>
    /// <param name="request">Payload describing the certificate to revoke.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeAsync(RevokeCertificateRequest request, CancellationToken cancellationToken = default) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        using var response = await _client.PostAsync("v1/certificate/revoke", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
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
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        using var response = await _client.PostAsync($"v1/certificate/renewById/{certificateId}", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<RenewCertificateResponse>(s_json, cancellationToken).ConfigureAwait(false);
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
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        using var response = await _client.PostAsync($"v1/certificate/renew/{orderNumber}", JsonContent.Create(request, options: s_json), cancellationToken).ConfigureAwait(false);
        var result = await response.Content.ReadFromJsonAsync<RenewCertificateResponse>(s_json, cancellationToken).ConfigureAwait(false);
        return result?.SslId ?? 0;
    }

    /// <summary>
    /// Searches for certificates using the provided filter.
    /// </summary>
    public async Task<CertificateResponse?> SearchAsync(
        CertificateSearchRequest request,
        CancellationToken cancellationToken = default) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        var list = new List<Certificate>();
        await foreach (var certificate in EnumerateSearchAsync(request, cancellationToken: cancellationToken).ConfigureAwait(false)) {
            list.Add(certificate);
        }

        return list.Count == 0 ? null : new CertificateResponse { Certificates = list };
    }

    /// <summary>
    /// Streams search results page by page.
    /// </summary>
    /// <param name="request">Filter describing certificates to retrieve.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async IAsyncEnumerable<Certificate> EnumerateSearchAsync(
        CertificateSearchRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        var originalSize = request.Size;
        var originalPosition = request.Position;
        var pageSize = request.Size ?? 200;
        var position = request.Position ?? 0;

        try {
            var query = BuildQuery(request);
            using var response = await _client.GetAsync($"v1/certificate{query}", cancellationToken).ConfigureAwait(false);
            var page = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<Certificate>>(s_json, cancellationToken)
                .ConfigureAwait(false);
            if (page is null || page.Count == 0) {
                yield break;
            }

            foreach (var certificate in page) {
                yield return certificate;
            }

            if (page.Count < pageSize) {
                yield break;
            }

            request.Size = pageSize;
            position += pageSize;

            while (true) {
                request.Position = position;
                query = BuildQuery(request);
                using var nextResponse = await _client.GetAsync($"v1/certificate{query}", cancellationToken).ConfigureAwait(false);
                page = await nextResponse.Content
                    .ReadFromJsonAsync<IReadOnlyList<Certificate>>(s_json, cancellationToken)
                    .ConfigureAwait(false);
                if (page is null || page.Count == 0) {
                    yield break;
                }

                foreach (var certificate in page) {
                    yield return certificate;
                }

                if (page.Count < pageSize) {
                    yield break;
                }

                position += pageSize;
            }
        } finally {
            request.Size = originalSize;
            request.Position = originalPosition;
        }
    }

    /// <summary>
    /// Downloads an issued certificate and saves it to disk.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to download.</param>
    /// <param name="path">Destination file path.</param>
    /// <param name="format">Certificate format to request. Defaults to <c>base64</c>.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DownloadAsync(
        int certificateId,
        string path,
        string format = "base64",
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }
        if (string.IsNullOrEmpty(path)) {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        var url = $"ssl/v1/collect/{certificateId}?format={Uri.EscapeDataString(format)}";
        using var response = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using (stream) {
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
#if NETSTANDARD2_0 || NET472
            await stream.CopyToAsync(file).ConfigureAwait(false);
#else
            await stream.CopyToAsync(file, cancellationToken).ConfigureAwait(false);
#endif
        }
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

        using var response = await _client.DeleteAsync($"v1/certificate/{certificateId}", cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
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
            builder.Append(name).Append('=').Append(value.Value.ToString("yyyy-MM-dd"));
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