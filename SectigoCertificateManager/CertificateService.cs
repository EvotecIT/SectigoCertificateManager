namespace SectigoCertificateManager;

using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides a unified facade for certificate operations across legacy and Admin APIs.
/// </summary>
public sealed class CertificateService : IDisposable {
    private readonly CertificatesClient? _legacyClient;
    private readonly AdminSslClient? _adminClient;
    private readonly ISectigoClient? _ownedLegacyClient;
    private readonly HttpClient? _ownedAdminHttpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateService"/> class using legacy API configuration.
    /// </summary>
    /// <param name="config">Legacy API configuration.</param>
    public CertificateService(ApiConfig config)
        : this(config, null) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateService"/> class using legacy API configuration.
    /// </summary>
    /// <param name="config">Legacy API configuration.</param>
    /// <param name="client">Optional Sectigo client instance.</param>
    public CertificateService(ApiConfig config, ISectigoClient? client) {
        Guard.AgainstNull(config, nameof(config));
        _ownedLegacyClient = client is null ? new SectigoClient(config) : null;
        var effective = client ?? _ownedLegacyClient!;
        _legacyClient = new CertificatesClient(effective);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateService"/> class using Admin API configuration.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    public CertificateService(AdminApiConfig config)
        : this(config, null) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CertificateService"/> class using Admin API configuration.
    /// </summary>
    /// <param name="config">Admin API configuration.</param>
    /// <param name="httpClient">Optional HTTP client.</param>
    public CertificateService(AdminApiConfig config, HttpClient? httpClient) {
        Guard.AgainstNull(config, nameof(config));
        _ownedAdminHttpClient = httpClient is null ? new HttpClient() : null;
        var effective = httpClient ?? _ownedAdminHttpClient!;
        _adminClient = new AdminSslClient(config, effective);
    }

    /// <summary>
    /// Lists certificates using the active API configuration.
    /// </summary>
    /// <param name="size">Number of certificates to retrieve.</param>
    /// <param name="position">Position offset for paging.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<Certificate>> ListAsync(
        int size,
        int position = 0,
        CancellationToken cancellationToken = default) {
        if (_adminClient is not null) {
            var identities = await _adminClient
                .ListAsync(size, position, cancellationToken)
                .ConfigureAwait(false);
            var list = new List<Certificate>(identities.Count);
            foreach (var identity in identities) {
                list.Add(MapIdentity(identity));
            }
            return list;
        }

        if (_legacyClient is not null) {
            var request = new CertificateSearchRequest { Size = size, Position = position };
            var response = await _legacyClient
                .SearchAsync(request, cancellationToken)
                .ConfigureAwait(false);
            return response?.Certificates ?? Array.Empty<Certificate>();
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    /// <summary>
    /// Retrieves a single certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Certificate?> GetAsync(
        int certificateId,
        CancellationToken cancellationToken = default) {
        if (_adminClient is not null) {
            var details = await _adminClient
                .GetAsync(certificateId, cancellationToken)
                .ConfigureAwait(false);
            return details is null ? null : MapDetails(details);
        }

        if (_legacyClient is not null) {
            return await _legacyClient
                .GetAsync(certificateId, cancellationToken)
                .ConfigureAwait(false);
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    private static Certificate MapIdentity(AdminSslIdentity identity) {
        return new Certificate {
            Id = identity.SslId,
            CommonName = identity.CommonName,
            SerialNumber = identity.SerialNumber,
            SubjectAlternativeNames = identity.SubjectAlternativeNames ?? Array.Empty<string>()
        };
    }

    private static Certificate MapDetails(AdminSslCertificateDetails details) {
        var certificate = new Certificate {
            Id = details.Id,
            CommonName = details.CommonName,
            OrgId = details.OrgId,
            BackendCertId = details.BackendCertId ?? string.Empty,
            Vendor = details.Vendor,
            Term = details.Term,
            Owner = details.Owner,
            Requester = details.Requester,
            Comments = details.Comments,
            Requested = details.Requested,
            Expires = details.Expires,
            SerialNumber = details.SerialNumber,
            KeyAlgorithm = details.KeyAlgorithm,
            KeySize = details.KeySize,
            KeyType = details.KeyType,
            SubjectAlternativeNames = details.SubjectAlternativeNames ?? Array.Empty<string>(),
            SuspendNotifications = details.SuspendNotifications
        };

        if (!string.IsNullOrWhiteSpace(details.Status)
            && Enum.TryParse<CertificateStatus>(details.Status.Replace(" ", string.Empty), ignoreCase: true, out var status)) {
            certificate.Status = status;
        }

        return certificate;
    }

    /// <summary>
    /// Retrieves the status of a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<CertificateStatus?> GetStatusAsync(
        int certificateId,
        CancellationToken cancellationToken = default) {
        if (_adminClient is not null) {
            var certificate = await GetAsync(certificateId, cancellationToken).ConfigureAwait(false);
            return certificate?.Status;
        }

        if (_legacyClient is not null) {
            return await _legacyClient
                .GetStatusAsync(certificateId, cancellationToken)
                .ConfigureAwait(false);
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    /// <summary>
    /// Retrieves revocation details for a certificate by identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<CertificateRevocation?> GetRevocationAsync(
        int certificateId,
        CancellationToken cancellationToken = default) {
        if (_adminClient is not null) {
            var details = await _adminClient
                .GetAsync(certificateId, cancellationToken)
                .ConfigureAwait(false);
            if (details is null) {
                return null;
            }

            var revocation = new CertificateRevocation {
                CertId = details.Id,
                SerialNumber = details.SerialNumber,
                RevokeDate = ParseDate(details.Revoked),
                ReasonCode = MapRevocationReason(details.ReasonCode),
                Reason = null
            };
            return revocation;
        }

        if (_legacyClient is not null) {
            return await _legacyClient
                .GetRevocationAsync(certificateId, cancellationToken)
                .ConfigureAwait(false);
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    private static DateTimeOffset? ParseDate(string? value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return null;
        }

        if (DateTimeOffset.TryParse(value, out var dto)) {
            return dto;
        }

        return null;
    }

    private static RevocationReason MapRevocationReason(string? code) {
        if (string.IsNullOrWhiteSpace(code)) {
            return RevocationReason.Unspecified;
        }

        return code.Trim() switch {
            "0" => RevocationReason.Unspecified,
            "1" => RevocationReason.KeyCompromise,
            "3" => RevocationReason.AffiliationChanged,
            "4" => RevocationReason.Superseded,
            "5" => RevocationReason.CessationOfOperation,
            _ => RevocationReason.Unspecified
        };
    }

    /// <inheritdoc />
    public void Dispose() {
        (_ownedLegacyClient as IDisposable)?.Dispose();
        _ownedAdminHttpClient?.Dispose();
    }
}
