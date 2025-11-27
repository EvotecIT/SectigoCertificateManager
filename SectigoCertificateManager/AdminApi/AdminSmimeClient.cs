namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
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
/// Minimal client for the Sectigo Admin Operations API S/MIME (client certificate) endpoints.
/// </summary>
public sealed class AdminSmimeClient : AdminApiClientBase {
    private static readonly JsonSerializerOptions s_json = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Initializes a new instance of the <see cref="AdminSmimeClient"/> class.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">
    /// Optional <see cref="HttpClient"/> instance. When not provided, a new instance is created
    /// and disposed with this client.
    /// </param>
    public AdminSmimeClient(AdminApiConfig config, HttpClient? httpClient = null)
        : base(config, httpClient) {
    }

    /// <summary>
    /// Lists client certificates using the Admin S/MIME API.
    /// </summary>
    /// <param name="size">Number of entries to request.</param>
    /// <param name="position">Position offset for paging.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<AdminSmimeCertificate>> ListAsync(
        int? size = null,
        int? position = null,
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = QueryStringBuilder.Build("api/smime/v2", q => q
            .AddInt("size", size)
            .AddInt("position", position));

        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var items = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminSmimeCertificate>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return items ?? Array.Empty<AdminSmimeCertificate>();
    }

    /// <summary>
    /// Retrieves detailed S/MIME certificate information by identifier.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSmimeCertificateDetails?> GetAsync(
        int certId,
        CancellationToken cancellationToken = default) {
        if (certId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/smime/v2/{certId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var details = await response.Content
            .ReadFromJsonAsyncSafe<AdminSmimeCertificateDetails>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return details;
    }

    /// <summary>
    /// Downloads an issued S/MIME certificate as a raw byte stream.
    /// </summary>
    /// <param name="backendCertId">Backend certificate identifier.</param>
    /// <param name="format">
    /// Optional format type. When <c>null</c>, the API default (<c>base64</c>) is used.
    /// Supported values are documented in the Admin API.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Stream> CollectAsync(
        string backendCertId,
        string? format = null,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(backendCertId, nameof(backendCertId));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = new StringBuilder($"api/smime/v2/collect/{Uri.EscapeDataString(backendCertId)}");
        if (!string.IsNullOrEmpty(format)) {
            path.Append("?format=").Append(Uri.EscapeDataString(format));
        }

        using var request = new HttpRequestMessage(HttpMethod.Get, path.ToString());
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient
            .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        return await response.Content.CopyToMemoryStreamAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Submits a request for a new S/MIME certificate using a CSR.
    /// </summary>
    /// <param name="request">Enrollment request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSmimeEnrollResponse?> EnrollAsync(
        AdminSmimeEnrollRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (request.OrgId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request.OrgId));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/smime/v2/enroll") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        await ApiErrorHandler.ThrowIfErrorAsync(response, cancellationToken).ConfigureAwait(false);

        var result = await response.Content
            .ReadFromJsonAsyncSafe<AdminSmimeEnrollResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Downloads a P12 keystore for the specified S/MIME certificate.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="request">Download parameters.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    /// The caller is responsible for disposing the returned <see cref="Stream"/>.
    /// </remarks>
    public async Task<Stream> DownloadPfxAsync(
        int certId,
        AdminSmimeP12DownloadRequest request,
        CancellationToken cancellationToken = default) {
        if (certId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certId));
        }

        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/smime/v2/keystore/{certId}") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient
            .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
            .ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.CopyToMemoryStreamAsync(cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Revokes all S/MIME certificates associated with a given email address.
    /// </summary>
    /// <param name="request">Revoke-by-email payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeByEmailAsync(
        AdminSmimeRevokeByEmailRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        Guard.AgainstNullOrWhiteSpace(request.Email, nameof(request.Email));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/smime/v2/revoke") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Renews a S/MIME certificate by backend certificate identifier.
    /// </summary>
    /// <param name="backendCertId">Backend certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSmimeEnrollResponse?> RenewByBackendIdAsync(
        string backendCertId,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(backendCertId, nameof(backendCertId));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/smime/v2/renew/order/{Uri.EscapeDataString(backendCertId)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsyncSafe<AdminSmimeEnrollResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Renews a S/MIME certificate by serial number.
    /// </summary>
    /// <param name="serialNumber">Certificate serial number.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<AdminSmimeEnrollResponse?> RenewBySerialAsync(
        string serialNumber,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(serialNumber, nameof(serialNumber));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/smime/v2/renew/serial/{Uri.EscapeDataString(serialNumber)}");
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsyncSafe<AdminSmimeEnrollResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Revokes a single S/MIME certificate by backend certificate identifier.
    /// </summary>
    /// <param name="backendCertId">Backend certificate identifier.</param>
    /// <param name="request">Revocation payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeByBackendIdAsync(
        string backendCertId,
        AdminSmimeRevokeRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(backendCertId, nameof(backendCertId));
        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/smime/v2/revoke/order/{Uri.EscapeDataString(backendCertId)}") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Revokes a single S/MIME certificate by serial number.
    /// </summary>
    /// <param name="serialNumber">Certificate serial number.</param>
    /// <param name="request">Revocation payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RevokeBySerialAsync(
        string serialNumber,
        AdminSmimeRevokeRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(serialNumber, nameof(serialNumber));
        Guard.AgainstNull(request, nameof(request));

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/smime/v2/revoke/serial/{Uri.EscapeDataString(serialNumber)}") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Marks a S/MIME certificate as revoked in SCM without contacting the CA.
    /// </summary>
    /// <param name="request">Mark-as-revoked payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task MarkAsRevokedAsync(
        AdminSmimeMarkAsRevokedRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));
        if (!request.CertId.HasValue && string.IsNullOrWhiteSpace(request.SerialNumber)) {
            throw new ArgumentException("Either CertId or SerialNumber must be provided.", nameof(request));
        }

        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var message = new HttpRequestMessage(HttpMethod.Post, "api/smime/v2/revoke/manual") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Lists S/MIME certificate profiles (types) using the Admin API.
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

        var path = new StringBuilder("api/smime/v2/types");
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
    /// Lists S/MIME custom fields using the Admin API.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<CustomField>> ListCustomFieldsAsync(
        CancellationToken cancellationToken = default) {
        var token = await GetAccessTokenAsync(cancellationToken).ConfigureAwait(false);

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/smime/v2/customFields");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var fields = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<CustomField>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return fields ?? Array.Empty<CustomField>();
    }

    /// <summary>
    /// Lists locations for the specified S/MIME certificate.
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

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/smime/v2/{certId}/location");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var list = await response.Content
            .ReadFromJsonAsyncSafe<IReadOnlyList<AdminSslLocation>>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return list ?? Array.Empty<AdminSslLocation>();
    }

    /// <summary>
    /// Retrieves details for a specific S/MIME certificate location.
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

        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/smime/v2/{certId}/location/{locationId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var location = await response.Content
            .ReadFromJsonAsyncSafe<AdminSslLocation>(s_json, cancellationToken)
            .ConfigureAwait(false);

        return location;
    }

    /// <summary>
    /// Creates a custom location for the specified S/MIME certificate.
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

        using var message = new HttpRequestMessage(HttpMethod.Post, $"api/smime/v2/{certId}/location") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return LocationHeaderParser.ParseId(response);
    }

    /// <summary>
    /// Updates a custom S/MIME certificate location.
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

        using var message = new HttpRequestMessage(HttpMethod.Put, $"api/smime/v2/{certId}/location/{locationId}") {
            Content = JsonContent.Create(request, options: s_json)
        };
        message.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

    /// <summary>
    /// Deletes a custom S/MIME certificate location.
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

        using var request = new HttpRequestMessage(HttpMethod.Delete, $"api/smime/v2/{certId}/location/{locationId}");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        using var response = await _httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }

}
