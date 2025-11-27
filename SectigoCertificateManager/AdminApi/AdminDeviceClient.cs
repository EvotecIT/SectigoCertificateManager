namespace SectigoCertificateManager.AdminApi;

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
/// Minimal client for the Sectigo Admin Operations API device certificate endpoints.
/// </summary>
public sealed class AdminDeviceClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminDeviceClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminDeviceClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists device certificates using the Admin API.
    /// </summary>
    /// <param name="size">Number of entries to request.</param>
    /// <param name="position">The first position to return from the result set.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminDeviceIdentity>> ListAsync(
        int? size = null,
        int? position = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var builder = new StringBuilder("api/device/v1");
        var hasQuery = false;

        void AppendInt(string name, int? value) {
            if (!value.HasValue) {
                return;
            }

            _ = hasQuery ? builder.Append('&') : builder.Append('?');
            builder.Append(name).Append('=').Append(value.Value);
            hasQuery = true;
        }

        AppendInt("size", size);
        AppendInt("position", position);

        using var request = new HttpRequestMessage(HttpMethod.Get, builder.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var identities = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminDeviceIdentity>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return identities ?? Array.Empty<AdminDeviceIdentity>();
    }

    /// <summary>
    /// Retrieves detailed device certificate information by identifier.
    /// </summary>
    /// <param name="deviceCertId">Device certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSslCertificateDetails?> GetAsync(
        int deviceCertId,
        CancellationToken cancellationToken = default) {
        if (deviceCertId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(deviceCertId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/device/v1/{deviceCertId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var details = await response.Content
            .ReadFromJsonAsyncSafe<AdminSslCertificateDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return details;
    }

    /// <summary>
    /// Downloads an issued device certificate as a raw byte stream.
    /// </summary>
    /// <param name="deviceCertId">Device certificate identifier.</param>
    /// <param name="format">
    /// Optional format type. When <c>null</c>, the API default (<c>base64</c>) is used.
    /// Supported values are documented in the Admin API.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Stream> CollectAsync(
        int deviceCertId,
        string? format = null,
        CancellationToken cancellationToken = default) {
        if (deviceCertId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(deviceCertId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = new StringBuilder($"api/device/v1/collect/{deviceCertId}");
        if (!string.IsNullOrEmpty(format)) {
            path.Append("?format=").Append(Uri.EscapeDataString(format));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var buffer = new MemoryStream();
#if NETSTANDARD2_0 || NET472
        await response.Content.CopyToAsync(buffer).ConfigureAwait(false);
#else
        await response.Content.CopyToAsync(buffer, cancellationToken).ConfigureAwait(false);
#endif
        buffer.Position = 0;
        return buffer;
    }

    /// <summary>
    /// Submits a request for a new device certificate using a CSR.
    /// </summary>
    /// <param name="request">Enrollment request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminDeviceEnrollResponse?> EnrollAsync(
        DeviceEnrollRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (request.OrgId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.OrgId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/device/v1/enroll") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsyncSafe<AdminDeviceEnrollResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Renews a device certificate by identifier.
    /// </summary>
    /// <param name="deviceCertId">Device certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminDeviceEnrollResponse?> RenewByIdAsync(
        int deviceCertId,
        CancellationToken cancellationToken = default) {
        if (deviceCertId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(deviceCertId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/device/v1/renew/order/{deviceCertId}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsyncSafe<AdminDeviceEnrollResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Renews a device certificate by serial number.
    /// </summary>
    /// <param name="serialNumber">Certificate serial number.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminDeviceEnrollResponse?> RenewBySerialAsync(
        string serialNumber,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(serialNumber, nameof(serialNumber));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/device/v1/renew/serial/{Uri.EscapeDataString(serialNumber)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsyncSafe<AdminDeviceEnrollResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Replaces a device certificate by identifier using a new CSR and the parameters of the initial certificate.
    /// </summary>
    /// <param name="deviceCertId">Device certificate identifier.</param>
    /// <param name="request">Replace request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task ReplaceAsync(
        int deviceCertId,
        AdminSslReplaceRequest request,
        CancellationToken cancellationToken = default) {
        if (deviceCertId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(deviceCertId));
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

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/device/v1/replace/order/{deviceCertId}") {
            Content = JsonContent.Create(body, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Revokes a device certificate by serial number.
    /// </summary>
    /// <param name="serialNumber">Certificate serial number.</param>
    /// <param name="reasonCode">Optional revocation reason code.</param>
    /// <param name="reason">Optional revocation reason text.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeBySerialAsync(
        string serialNumber,
        string? reasonCode = null,
        string? reason = null,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(serialNumber, nameof(serialNumber));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new DeviceRevokeRequest {
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode,
            Reason = reason
        };

        using var message = new HttpRequestMessage(
            HttpMethod.Post,
            $"api/device/v1/revoke/serial/{Uri.EscapeDataString(serialNumber)}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Imports device certificates using the JSON-based Admin Operations import endpoint.
    /// </summary>
    /// <param name="requests">A collection of certificate import requests.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<CertificateImportResult>> ImportAsync(
        IReadOnlyList<CertificateImportRequest> requests,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(requests, nameof(requests));
        if (requests.Count == 0) {
            throw new ArgumentException("At least one import request must be provided.", nameof(requests));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/device/v1/import") {
            Content = JsonContent.Create(requests, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var results = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<CertificateImportResult>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return results ?? Array.Empty<CertificateImportResult>();
    }

    /// <summary>
    /// Revokes a device certificate by identifier.
    /// </summary>
    /// <param name="deviceCertId">Device certificate identifier.</param>
    /// <param name="reasonCode">Optional revocation reason code.</param>
    /// <param name="reason">Optional revocation reason text.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeByIdAsync(
        int deviceCertId,
        string? reasonCode = null,
        string? reason = null,
        CancellationToken cancellationToken = default) {
        if (deviceCertId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(deviceCertId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new DeviceRevokeRequest {
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode,
            Reason = reason
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/device/v1/revoke/order/{deviceCertId}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Marks a device certificate as revoked in SCM without contacting the CA.
    /// </summary>
    /// <param name="deviceCertId">Optional device certificate identifier.</param>
    /// <param name="serialNumber">Optional certificate serial number.</param>
    /// <param name="issuer">Optional certificate issuer used together with <paramref name="serialNumber"/>.</param>
    /// <param name="revokeDate">Optional revocation date.</param>
    /// <param name="reasonCode">Optional revocation reason code.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task MarkAsRevokedAsync(
        int? deviceCertId = null,
        string? serialNumber = null,
        string? issuer = null,
        DateTimeOffset? revokeDate = null,
        string? reasonCode = null,
        CancellationToken cancellationToken = default) {
        if (!deviceCertId.HasValue && string.IsNullOrWhiteSpace(serialNumber)) {
            throw new ArgumentException("Either deviceCertId or serialNumber must be provided.");
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new MarkAsRevokedRequest {
            CertId = deviceCertId,
            SerialNumber = serialNumber,
            Issuer = issuer,
            RevokeDate = revokeDate,
            ReasonCode = string.IsNullOrWhiteSpace(reasonCode) ? "0" : reasonCode
        };

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/device/v1/revoke/manual") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Approves a device certificate request that requires approval.
    /// </summary>
    /// <param name="deviceCertId">Device certificate identifier.</param>
    /// <param name="message">Optional message containing additional information about the approval action.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task ApproveAsync(
        int deviceCertId,
        string? message = null,
        CancellationToken cancellationToken = default) {
        if (deviceCertId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(deviceCertId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new ApproveDeclineRequest {
            Message = message
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/device/v1/approve/{deviceCertId}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Declines a device certificate request.
    /// </summary>
    /// <param name="deviceCertId">Device certificate identifier.</param>
    /// <param name="message">Optional message containing additional information about the decline action.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeclineAsync(
        int deviceCertId,
        string? message = null,
        CancellationToken cancellationToken = default) {
        if (deviceCertId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(deviceCertId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var payload = new ApproveDeclineRequest {
            Message = message
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/device/v1/decline/{deviceCertId}") {
            Content = JsonContent.Create(payload, options: s_json)
        };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Lists device certificate profiles (types) using the Admin API.
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

        var path = new StringBuilder("api/device/v1/types");
        if (organizationId.HasValue) {
            path.Append("?organizationId=").Append(organizationId.Value);
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var types = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<CertificateType>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return types ?? Array.Empty<CertificateType>();
    }

    /// <summary>
    /// Lists device custom fields using the Admin API.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<CustomField>> ListCustomFieldsAsync(
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/device/v1/customFields");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var fields = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<CustomField>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return fields ?? Array.Empty<CustomField>();
    }

    /// <summary>
    /// Lists locations for the specified device certificate.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminSslLocation>> ListLocationsAsync(
        int certId,
        CancellationToken cancellationToken = default) {
        if (certId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/device/v1/{certId}/location");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var list = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminSslLocation>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return list ?? Array.Empty<AdminSslLocation>();
    }

    /// <summary>
    /// Retrieves details for a specific device certificate location.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="locationId">Location identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSslLocation?> GetLocationAsync(
        int certId,
        int locationId,
        CancellationToken cancellationToken = default) {
        if (certId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certId));
        }

        if (locationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(locationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/device/v1/{certId}/location/{locationId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var location = await response.Content
            .ReadFromJsonAsyncSafe<AdminSslLocation>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return location;
    }

    /// <summary>
    /// Creates a custom location for the specified device certificate.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="request">Location details.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The identifier of the created location, or 0 when not available.</returns>
    public async Task<int> CreateLocationAsync(
        int certId,
        AdminSslLocationRequest request,
        CancellationToken cancellationToken = default) {
        if (certId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/device/v1/{certId}/location") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var location = response.Headers.Location;
        if (location is not null) {
            var url = location.ToString().Trim().TrimEnd('/');
            var segments = url.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 0 && int.TryParse(segments[segments.Length - 1], out var id)) {
                return id;
            }
        }

        return 0;
    }

    /// <summary>
    /// Updates a custom device certificate location.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="locationId">Location identifier.</param>
    /// <param name="request">Updated location details.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task UpdateLocationAsync(
        int certId,
        int locationId,
        AdminSslLocationRequest request,
        CancellationToken cancellationToken = default) {
        if (certId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certId));
        }

        if (locationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(locationId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/device/v1/{certId}/location/{locationId}") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes a custom device certificate location.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="locationId">Location identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DeleteLocationAsync(
        int certId,
        int locationId,
        CancellationToken cancellationToken = default) {
        if (certId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certId));
        }

        if (locationId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(locationId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/device/v1/{certId}/location/{locationId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    private sealed class DeviceRevokeRequest {
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
}
