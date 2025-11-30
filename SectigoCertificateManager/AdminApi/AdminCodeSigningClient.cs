namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Client for the Admin Operations API code-signing certificate endpoints.
/// </summary>
public sealed class AdminCodeSigningClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminCodeSigningClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminCodeSigningClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Imports one or more code-signing certificates.
    /// </summary>
    /// <param name="requests">Collection of import requests describing the certificates to import.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A collection of <see cref="CertificateImportResult"/> entries describing the outcome of each import.</returns>
    public async Task<IReadOnlyList<CertificateImportResult>> ImportAsync(
        IReadOnlyList<CertificateImportRequest> requests,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(requests, nameof(requests));

        if (requests.Count == 0) {
            return Array.Empty<CertificateImportResult>();
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/cscert/v1/import") {
            Content = JsonContent.Create(requests, options: s_json)
        };
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var results = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<CertificateImportResult>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return results ?? Array.Empty<CertificateImportResult>();
    }

    /// <summary>
    /// Marks a code-signing certificate as revoked using either its identifier or serial number and issuer.
    /// </summary>
    /// <param name="certId">Optional certificate identifier.</param>
    /// <param name="serialNumber">Optional certificate serial number.</param>
    /// <param name="issuer">Optional certificate issuer distinguished name, required when <paramref name="serialNumber"/> is used.</param>
    /// <param name="revokeDate">Optional revocation date. When <c>null</c>, the server default is used.</param>
    /// <param name="reasonCode">
    /// Optional revocation reason code string as defined by the Admin API
    /// (for example, "0", "1", "3", "4", "5"). When <c>null</c>, "0" (unspecified) is used.
    /// </param>
    /// <param name="reason">Optional text describing the revocation reason.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeManualAsync(
        int? certId = null,
        string? serialNumber = null,
        string? issuer = null,
        System.DateTimeOffset? revokeDate = null,
        string? reasonCode = null,
        string? reason = null,
        CancellationToken cancellationToken = default) {
        if (!certId.HasValue && string.IsNullOrWhiteSpace(serialNumber)) {
            throw new System.ArgumentException("Either certId or serialNumber must be provided.");
        }

        if (!string.IsNullOrWhiteSpace(serialNumber) && string.IsNullOrWhiteSpace(issuer)) {
            throw new System.ArgumentException("Issuer must be provided when serialNumber is specified.");
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new MarkAsRevokedRequest {
            CertId = certId,
            SerialNumber = serialNumber,
            Issuer = issuer,
            RevokeDate = revokeDate,
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode,
            Reason = reason
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/cscert/v1/revoke/manual") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    private sealed class MarkAsRevokedRequest {
        [JsonPropertyName("certId")]
        public int? CertId { get; set; }

        [JsonPropertyName("serialNumber")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        [JsonPropertyName("revokeDate")]
        public System.DateTimeOffset? RevokeDate { get; set; }

        [JsonPropertyName("reasonCode")]
        public string? ReasonCode { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }
}

