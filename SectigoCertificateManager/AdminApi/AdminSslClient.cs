namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager.Utilities;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Minimal client for the Sectigo Admin Operations API SSL endpoints.
/// </summary>
public sealed class AdminSslClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSslClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminSslClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists SSL certificates using the Admin API.
    /// </summary>
    /// <param name="size">Number of entries to request.</param>
    /// <param name="position">The first position to return from the result set.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminSslIdentity>> ListAsync(
        int? size = null,
        int? position = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, BuildListUri(size, position));
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var identities = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminSslIdentity>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return identities ?? Array.Empty<AdminSslIdentity>();
    }

    /// <summary>
    /// Retrieves detailed SSL certificate information by identifier.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSslCertificateDetails?> GetAsync(
        int sslId,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/ssl/v2/{sslId}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var details = await response.Content
            .ReadFromJsonAsyncSafe<AdminSslCertificateDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return details;
    }

    /// <summary>
    /// Revokes an SSL certificate by identifier.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="reasonCode">
    /// Optional revocation reason code string as defined by the Admin API
    /// (for example, "0", "1", "3", "4", "5").
    /// When <c>null</c>, "0" (unspecified) is used.
    /// </param>
    /// <param name="reason">Optional human-readable revocation reason.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeByIdAsync(
        int sslId,
        string? reasonCode = null,
        string? reason = null,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var payload = new RevokeRequest {
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode,
            Reason = reason
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/revoke/{sslId}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Revokes an SSL certificate by serial number.
    /// </summary>
    /// <param name="serialNumber">Certificate serial number.</param>
    /// <param name="reasonCode">
    /// Optional revocation reason code string as defined by the Admin API
    /// (for example, "0", "1", "3", "4", "5").
    /// When <c>null</c>, "0" (unspecified) is used.
    /// </param>
    /// <param name="reason">Optional human-readable revocation reason.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeBySerialAsync(
        string serialNumber,
        string? reasonCode = null,
        string? reason = null,
        CancellationToken cancellationToken = default) {
        if (string.IsNullOrWhiteSpace(serialNumber)) {
            throw new ArgumentException("Serial number cannot be null or empty.", nameof(serialNumber));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);
        var payload = new RevokeRequest {
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode,
            Reason = reason
        };

        var encodedSerial = Uri.EscapeDataString(serialNumber);
        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/revoke/serial/{encodedSerial}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Approves an SSL certificate request that requires approval.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="message">
    /// Optional message containing additional information about the approval action.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task ApproveAsync(
        int sslId,
        string? message = null,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new ApproveDeclineRequest {
            Message = message
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/approve/{sslId}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Declines an SSL certificate request.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="message">
    /// Optional message containing additional information about the decline action.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeclineAsync(
        int sslId,
        string? message = null,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new ApproveDeclineRequest {
            Message = message
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/decline/{sslId}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Marks an SSL certificate as revoked in SCM without contacting the CA.
    /// </summary>
    /// <param name="certId">Optional certificate identifier.</param>
    /// <param name="serialNumber">Optional certificate serial number.</param>
    /// <param name="issuer">Optional certificate issuer used together with <paramref name="serialNumber"/>.</param>
    /// <param name="revokeDate">Optional revocation date.</param>
    /// <param name="reasonCode">
    /// Optional revocation reason code string as defined by the Admin API
    /// (for example, "0", "1", "3", "4", "5").
    /// When <c>null</c>, "0" (unspecified) is used.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task MarkAsRevokedAsync(
        int? certId = null,
        string? serialNumber = null,
        string? issuer = null,
        DateTimeOffset? revokeDate = null,
        string? reasonCode = null,
        CancellationToken cancellationToken = default) {
        if (!certId.HasValue && string.IsNullOrWhiteSpace(serialNumber)) {
            throw new ArgumentException("Either certId or serialNumber must be provided.");
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new MarkAsRevokedRequest {
            CertId = certId,
            SerialNumber = serialNumber,
            Issuer = issuer,
            RevokeDate = revokeDate,
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/ssl/v2/revoke/manual") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Downloads an issued certificate as a raw byte stream.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="format">
    /// Optional format type. When <c>null</c>, the API default (<c>base64</c>) is used.
    /// Supported values are documented in the Admin API (for example, <c>base64</c>, <c>x509</c>, <c>pem</c>).
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    /// The caller is responsible for disposing the returned <see cref="Stream"/>.
    /// </remarks>
    public async Task<Stream> CollectAsync(
        int sslId,
        string? format = null,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = new StringBuilder($"api/ssl/v2/collect/{sslId}");
        if (!string.IsNullOrEmpty(format)) {
            path.Append("?format=").Append(Uri.EscapeDataString(format));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path.ToString());
        SetBearer(request, token);

        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content.CopyToMemoryStreamAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Renews an SSL certificate by identifier.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="request">Renewal request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<int> RenewByIdAsync(
        int sslId,
        RenewCertificateRequest request,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var body = new RenewInfo {
            Csr = request.Csr,
            DcvMode = request.DcvMode,
            DcvEmail = request.DcvEmail
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/renewById/{sslId}") {
            Content = JsonContent.Create(body, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content
            .ReadFromJsonAsyncSafe<RenewCertificateResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        return result?.SslId ?? 0;
    }

    /// <summary>
    /// Renews an SSL certificate by renew identifier.
    /// </summary>
    /// <param name="renewId">Renew identifier associated with the certificate.</param>
    /// <param name="request">Renewal request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RenewByRenewIdAsync(
        string renewId,
        RenewCertificateRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(renewId, nameof(renewId));
        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var body = new RenewInfo {
            Csr = request.Csr,
            DcvMode = request.DcvMode,
            DcvEmail = request.DcvEmail
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/renew/{renewId}") {
            Content = JsonContent.Create(body, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Submits a request for a new SSL certificate using an existing CSR.
    /// </summary>
    /// <param name="request">Enrollment request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSslEnrollResponse?> EnrollAsync(
        AdminSslEnrollRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (request.OrgId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.OrgId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/ssl/v2/enroll") {
            Content = JsonContent.Create(request, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<AdminSslEnrollResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Submits a request for a new SSL certificate with server-side key generation.
    /// </summary>
    /// <param name="request">Enrollment request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSslEnrollResponse?> EnrollWithKeyGenerationAsync(
        AdminSslEnrollKeyGenRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (request.OrgId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.OrgId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/ssl/v2/enroll-keygen") {
            Content = JsonContent.Create(request, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<AdminSslEnrollResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Imports certificates into SCM using a zip archive.
    /// </summary>
    /// <param name="orgId">Organization identifier.</param>
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

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/zip");
        content.Add(fileContent, "file", fileName);

        var path = $"api/ssl/v2/import?orgId={orgId}";
        using var message = new HttpRequestMessage(HttpMethod.Post, path) {
            Content = content
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content
            .ReadFromJsonAsyncSafe<ImportCertificateResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Submits a manual renewal request for an SSL certificate.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="request">Manual renewal request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RenewManualAsync(
        int sslId,
        AdminSslManualRenewRequest request,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var body = new RenewManualBody {
            Id = request.Id > 0 ? request.Id : sslId,
            OrderNumber = request.OrderNumber,
            DcvMode = request.DcvMode,
            DcvEmail = request.DcvEmail
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/renew/manual/{sslId}") {
            Content = JsonContent.Create(body, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Replaces an SSL certificate by identifier.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="request">Replace request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task ReplaceAsync(
        int sslId,
        AdminSslReplaceRequest request,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var body = new ReplaceBody {
            Csr = request.Csr,
            Reason = request.Reason,
            CommonName = request.CommonName,
            SubjectAlternativeNames = request.SubjectAlternativeNames ?? Array.Empty<string>(),
            DcvMode = request.DcvMode,
            DcvEmail = request.DcvEmail
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/replace/{sslId}") {
            Content = JsonContent.Create(body, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Creates a keystore download link for the specified certificate.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="formatType">
    /// Keystore format type as defined by the Admin API (for example, <c>key</c>, <c>p12</c>, <c>p12aes</c>, <c>jks</c>, <c>pem</c>).
    /// </param>
    /// <param name="passphrase">Optional passphrase used to protect the keystore.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<string> CreateKeystoreLinkAsync(
        int sslId,
        string formatType,
        string? passphrase = null,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        if (string.IsNullOrWhiteSpace(formatType)) {
            throw new ArgumentException("Format type cannot be null or empty.", nameof(formatType));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new DownloadLinkRequest {
            Passphrase = passphrase
        };

        var path = $"api/ssl/v2/keystore/{sslId}/{Uri.EscapeDataString(formatType)}";
        using var message = new HttpRequestMessage(HttpMethod.Post, path) {
            Content = JsonContent.Create(payload, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content
            .ReadFromJsonAsyncSafe<DownloadFromPkResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result?.Link ?? string.Empty;
    }

    /// <summary>
    /// Retrieves DCV information for the specified certificate.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminSslDcvInfo>> GetDcvInfoAsync(
        int sslId,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/ssl/v2/{sslId}/dcv");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminSslDcvInfo>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminSslDcvInfo>();
    }

    /// <summary>
    /// Initiates DCV revalidation for the specified certificate and returns the updated DCV info.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSslDcvInfo?> RecheckDcvAsync(
        int sslId,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/{sslId}/dcv/recheck");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var info = await response.Content
            .ReadFromJsonAsyncSafe<AdminSslDcvInfo>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return info;
    }

    /// <summary>
    /// Lists locations for the specified certificate.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminSslLocation>> ListLocationsAsync(
        int sslId,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/ssl/v2/{sslId}/location");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var list = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminSslLocation>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return list ?? Array.Empty<AdminSslLocation>();
    }

    /// <summary>
    /// Retrieves details for a specific certificate location.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="locationId">Location identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSslLocation?> GetLocationAsync(
        int sslId,
        int locationId,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }
        if (locationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(locationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/ssl/v2/{sslId}/location/{locationId}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var location = await response.Content
            .ReadFromJsonAsyncSafe<AdminSslLocation>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return location;
    }

    /// <summary>
    /// Creates a custom location for the specified certificate.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="request">Location details.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created location, or 0 when not available.</returns>
    public async Task<int> CreateLocationAsync(
        int sslId,
        AdminSslLocationRequest request,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/ssl/v2/{sslId}/location") {
            Content = JsonContent.Create(request, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return LocationHeaderParser.ParseId(response);
    }

    /// <summary>
    /// Updates a custom certificate location.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="locationId">Location identifier.</param>
    /// <param name="request">Updated location details.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task UpdateLocationAsync(
        int sslId,
        int locationId,
        AdminSslLocationRequest request,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }
        if (locationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(locationId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/ssl/v2/{sslId}/location/{locationId}") {
            Content = JsonContent.Create(request, options: s_json)
        };
        SetBearer(message, token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Deletes a custom certificate location.
    /// </summary>
    /// <param name="sslId">Certificate identifier.</param>
    /// <param name="locationId">Location identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteLocationAsync(
        int sslId,
        int locationId,
        CancellationToken cancellationToken = default) {
        if (sslId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(sslId));
        }
        if (locationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(locationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/ssl/v2/{sslId}/location/{locationId}");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Lists SSL certificate profiles (types) using the Admin API.
    /// </summary>
    /// <param name="organizationId">Optional organization identifier filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<CertificateType>> ListCertificateTypesAsync(
        int? organizationId = null,
        CancellationToken cancellationToken = default) {
        if (organizationId.HasValue && organizationId.Value <= 0) {
            throw new ArgumentOutOfRangeException(nameof(organizationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = new StringBuilder("api/ssl/v2/types");
        if (organizationId.HasValue) {
            path.Append("?organizationId=").Append(organizationId.Value);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path.ToString());
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var types = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<CertificateType>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return types ?? Array.Empty<CertificateType>();
    }

    /// <summary>
    /// Lists SSL certificate custom fields using the Admin API.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<CustomField>> ListCustomFieldsAsync(
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/ssl/v2/customFields");
        SetBearer(request, token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var fields = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<CustomField>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return fields ?? Array.Empty<CustomField>();
    }

    private string BuildListUri(int? size, int? position) {
        return QueryStringBuilder.Build("api/ssl/v2", q => q
            .AddInt("size", size)
            .AddInt("position", position));
    }

    private sealed class RenewInfo {
        [JsonPropertyName("csr")]
        public string? Csr { get; set; }

        [JsonPropertyName("dcvMode")]
        public string? DcvMode { get; set; }

        [JsonPropertyName("dcvEmail")]
        public string? DcvEmail { get; set; }
    }

    private sealed class RenewManualBody {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("orderNumber")]
        public string? OrderNumber { get; set; }

        [JsonPropertyName("dcvMode")]
        public string? DcvMode { get; set; }

        [JsonPropertyName("dcvEmail")]
        public string? DcvEmail { get; set; }
    }

    private sealed class ReplaceBody {
        [JsonPropertyName("csr")]
        public string Csr { get; set; } = string.Empty;

        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;

        [JsonPropertyName("commonName")]
        public string? CommonName { get; set; }

        [JsonPropertyName("subjectAlternativeNames")]
        public IReadOnlyList<string> SubjectAlternativeNames { get; set; } = Array.Empty<string>();

        [JsonPropertyName("dcvMode")]
        public string? DcvMode { get; set; }

        [JsonPropertyName("dcvEmail")]
        public string? DcvEmail { get; set; }
    }

    private sealed class RevokeRequest {
        [JsonPropertyName("reasonCode")]
        public string? ReasonCode { get; set; }

        [JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }

    private sealed class MarkAsRevokedRequest {
        [JsonPropertyName("certId")]
        public int? CertId { get; set; }

        [JsonPropertyName("serialNumber")]
        public string? SerialNumber { get; set; }

        [JsonPropertyName("issuer")]
        public string? Issuer { get; set; }

        [JsonPropertyName("revokeDate")]
        public DateTimeOffset? RevokeDate { get; set; }

        [JsonPropertyName("reasonCode")]
        public string? ReasonCode { get; set; }
    }

    private sealed class ApproveDeclineRequest {
        [JsonPropertyName("message")]
        public string? Message { get; set; }
    }

    private sealed class DownloadLinkRequest {
        [JsonPropertyName("passphrase")]
        public string? Passphrase { get; set; }
    }

    private sealed class DownloadFromPkResponse {
        [JsonPropertyName("link")]
        public string? Link { get; set; }
    }
}
