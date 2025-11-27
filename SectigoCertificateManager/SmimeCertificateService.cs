namespace SectigoCertificateManager;

using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides a facade for S/MIME certificate operations using the Admin API.
/// </summary>
public sealed class SmimeCertificateService : IDisposable {
    private readonly AdminSmimeClient _adminClient;
    private readonly AdminSmimeClient? _ownedAdminClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="SmimeCertificateService"/> class using Admin API configuration.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    public SmimeCertificateService(AdminApiConfig config)
        : this(config, null) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SmimeCertificateService"/> class using Admin API configuration.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">Optional HTTP client instance.</param>
    public SmimeCertificateService(AdminApiConfig config, HttpClient? httpClient) {
        Guard.AgainstNull(config, nameof(config));
        if (httpClient is null) {
            _ownedAdminClient = new AdminSmimeClient(config);
            _adminClient = _ownedAdminClient;
        } else {
            _adminClient = new AdminSmimeClient(config, httpClient);
        }
    }

    /// <summary>
    /// Lists S/MIME certificates using the Admin API.
    /// </summary>
    /// <param name="size">Optional page size.</param>
    /// <param name="position">Optional position offset.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<IReadOnlyList<AdminSmimeCertificate>> ListAsync(
        int? size = null,
        int? position = null,
        CancellationToken cancellationToken = default) {
        return _adminClient.ListAsync(size, position, cancellationToken);
    }

    /// <summary>
    /// Retrieves detailed S/MIME certificate information by identifier.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<AdminSmimeCertificateDetails?> GetAsync(
        int certId,
        CancellationToken cancellationToken = default) {
        return _adminClient.GetAsync(certId, cancellationToken);
    }

    /// <summary>
    /// Downloads an issued S/MIME certificate and returns it as an X509Certificate2 instance.
    /// </summary>
    /// <param name="backendCertId">Backend certificate identifier.</param>
    /// <param name="format">
    /// Optional format type. When <c>null</c>, the API default (<c>base64</c>) is used.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<System.Security.Cryptography.X509Certificates.X509Certificate2> DownloadCertificateAsync(
        string backendCertId,
        string? format = null,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNullOrWhiteSpace(backendCertId, nameof(backendCertId));

        using var stream = await _adminClient
            .CollectAsync(backendCertId, format, cancellationToken)
            .ConfigureAwait(false);
        return Certificate.FromBase64(stream);
    }

    /// <summary>
    /// Downloads a PFX keystore for the specified S/MIME certificate.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="request">Download parameters.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<System.IO.Stream> DownloadPfxAsync(
        int certId,
        AdminSmimeP12DownloadRequest request,
        CancellationToken cancellationToken = default) {
        return _adminClient.DownloadPfxAsync(certId, request, cancellationToken);
    }

    /// <summary>
    /// Submits a request for a new S/MIME certificate using a CSR.
    /// </summary>
    /// <param name="request">Enrollment request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<AdminSmimeEnrollResponse?> EnrollAsync(
        AdminSmimeEnrollRequest request,
        CancellationToken cancellationToken = default) {
        return _adminClient.EnrollAsync(request, cancellationToken);
    }

    /// <summary>
    /// Renews a S/MIME certificate by backend certificate identifier.
    /// </summary>
    /// <param name="backendCertId">Backend certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<AdminSmimeEnrollResponse?> RenewByBackendIdAsync(
        string backendCertId,
        CancellationToken cancellationToken = default) {
        return _adminClient.RenewByBackendIdAsync(backendCertId, cancellationToken);
    }

    /// <summary>
    /// Renews a S/MIME certificate by serial number.
    /// </summary>
    /// <param name="serialNumber">Certificate serial number.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<AdminSmimeEnrollResponse?> RenewBySerialAsync(
        string serialNumber,
        CancellationToken cancellationToken = default) {
        return _adminClient.RenewBySerialAsync(serialNumber, cancellationToken);
    }

    /// <summary>
    /// Revokes S/MIME certificates by email address.
    /// </summary>
    /// <param name="request">Revoke-by-email request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task RevokeByEmailAsync(
        AdminSmimeRevokeByEmailRequest request,
        CancellationToken cancellationToken = default) {
        return _adminClient.RevokeByEmailAsync(request, cancellationToken);
    }

    /// <summary>
    /// Revokes a S/MIME certificate by backend certificate identifier.
    /// </summary>
    /// <param name="backendCertId">Backend certificate identifier.</param>
    /// <param name="request">Revocation payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task RevokeByBackendIdAsync(
        string backendCertId,
        AdminSmimeRevokeRequest request,
        CancellationToken cancellationToken = default) {
        return _adminClient.RevokeByBackendIdAsync(backendCertId, request, cancellationToken);
    }

    /// <summary>
    /// Revokes a S/MIME certificate by serial number.
    /// </summary>
    /// <param name="serialNumber">Certificate serial number.</param>
    /// <param name="request">Revocation payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task RevokeBySerialAsync(
        string serialNumber,
        AdminSmimeRevokeRequest request,
        CancellationToken cancellationToken = default) {
        return _adminClient.RevokeBySerialAsync(serialNumber, request, cancellationToken);
    }

    /// <summary>
    /// Marks a S/MIME certificate as revoked in SCM without contacting the CA.
    /// </summary>
    /// <param name="request">Mark-as-revoked payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task MarkAsRevokedAsync(
        AdminSmimeMarkAsRevokedRequest request,
        CancellationToken cancellationToken = default) {
        return _adminClient.MarkAsRevokedAsync(request, cancellationToken);
    }

    /// <summary>
    /// Lists S/MIME certificate profiles (types).
    /// </summary>
    /// <param name="organizationId">Optional organization identifier filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<IReadOnlyList<CertificateType>> ListCertificateTypesAsync(
        int? organizationId = null,
        CancellationToken cancellationToken = default) {
        return _adminClient.ListCertificateTypesAsync(organizationId, cancellationToken);
    }

    /// <summary>
    /// Lists S/MIME custom fields.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<IReadOnlyList<CustomField>> ListCustomFieldsAsync(
        CancellationToken cancellationToken = default) {
        return _adminClient.ListCustomFieldsAsync(cancellationToken);
    }

    /// <summary>
    /// Lists locations for the specified S/MIME certificate.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<IReadOnlyList<AdminSslLocation>> ListLocationsAsync(
        int certId,
        CancellationToken cancellationToken = default) {
        return _adminClient.ListLocationsAsync(certId, cancellationToken);
    }

    /// <summary>
    /// Retrieves details for a specific S/MIME certificate location.
    /// </summary>
    /// <param name="certId">Certificate identifier.</param>
    /// <param name="locationId">Location identifier.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<AdminSslLocation?> GetLocationAsync(
        int certId,
        int locationId,
        CancellationToken cancellationToken = default) {
        return _adminClient.GetLocationAsync(certId, locationId, cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose() {
        _ownedAdminClient?.Dispose();
    }
}

