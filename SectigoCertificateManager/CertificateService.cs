namespace SectigoCertificateManager;

using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides a unified facade for certificate operations across legacy and Admin APIs.
/// </summary>
public sealed class CertificateService : IDisposable {
    private readonly CertificatesClient? _legacyClient;
    private readonly AdminSslClient? _adminClient;
    private readonly ISectigoClient? _ownedLegacyClient;
    private readonly AdminSslClient? _ownedAdminClient;

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
        if (httpClient is null) {
            _ownedAdminClient = new AdminSslClient(config);
            _adminClient = _ownedAdminClient;
        } else {
            _adminClient = new AdminSslClient(config, httpClient);
        }
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
    /// Streams certificates using the active API configuration.
    /// </summary>
    /// <param name="pageSize">Number of certificates to request per page.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async IAsyncEnumerable<Certificate> EnumerateAsync(
        int pageSize = 200,
        [EnumeratorCancellation] CancellationToken cancellationToken = default) {
        if (pageSize <= 0) {
            throw new ArgumentOutOfRangeException(nameof(pageSize));
        }

        if (_adminClient is not null) {
            var position = 0;
            while (true) {
                var page = await _adminClient
                    .ListAsync(pageSize, position, cancellationToken)
                    .ConfigureAwait(false);
                if (page is null || page.Count == 0) {
                    yield break;
                }

                foreach (var identity in page) {
                    yield return MapIdentity(identity);
                }

                if (page.Count < pageSize) {
                    yield break;
                }

                position += pageSize;
            }
        }
        else if (_legacyClient is not null) {
            var request = new CertificateSearchRequest { Size = pageSize };
            await foreach (var certificate in _legacyClient
                .EnumerateSearchAsync(request, cancellationToken)
                .ConfigureAwait(false)) {
                yield return certificate;
            }
        }
        else {
            throw new InvalidOperationException("No underlying client is configured for CertificateService.");
        }
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

        if (details.Status is string statusText && !string.IsNullOrWhiteSpace(statusText)) {
            var normalized = statusText.Replace(" ", string.Empty);
            if (Enum.TryParse<CertificateStatus>(normalized, ignoreCase: true, out var status)) {
                certificate.Status = status;
            }
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

    /// <summary>
    /// Downloads an issued certificate and returns an <see cref="System.Security.Cryptography.X509Certificates.X509Certificate2"/> instance.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="format">
    /// Optional format parameter for Admin API downloads. When <c>null</c>, the API default (<c>base64</c>) is used.
    /// Ignored for legacy API calls, which always request base64.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<System.Security.Cryptography.X509Certificates.X509Certificate2> DownloadCertificateAsync(
        int certificateId,
        string? format = null,
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        if (_adminClient is not null) {
            using var stream = await _adminClient
                .CollectAsync(certificateId, format, cancellationToken)
                .ConfigureAwait(false);
            return Models.Certificate.FromBase64(stream);
        }

        if (_legacyClient is not null) {
            return await _legacyClient
                .DownloadAsync(certificateId, cancellationToken)
                .ConfigureAwait(false);
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    /// <summary>
    /// Creates a keystore download link for a certificate using the Admin API.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="formatType">
    /// Keystore format type as defined by the Admin API (for example, <c>key</c>, <c>p12</c>, <c>p12aes</c>, <c>jks</c>, <c>pem</c>).
    /// </param>
    /// <param name="passphrase">Optional passphrase used to protect the keystore.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>A keystore download link.</returns>
    /// <remarks>
    /// This operation is only supported when the service is configured with an Admin API configuration.
    /// When configured with a legacy API configuration, a <see cref="NotSupportedException"/> is thrown.
    /// </remarks>
    public async Task<string> CreateKeystoreDownloadLinkAsync(
        int certificateId,
        string formatType,
        string? passphrase = null,
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        if (string.IsNullOrWhiteSpace(formatType)) {
            throw new ArgumentException("Format type cannot be null or empty.", nameof(formatType));
        }

        if (_adminClient is not null) {
            return await _adminClient
                .CreateKeystoreLinkAsync(certificateId, formatType, passphrase, cancellationToken)
                .ConfigureAwait(false);
        }

        if (_legacyClient is not null) {
            throw new NotSupportedException("Keystore download is only supported when using the Admin Operations API configuration.");
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

        var value = code?.Trim() ?? string.Empty;
        return value switch {
            "0" => RevocationReason.Unspecified,
            "1" => RevocationReason.KeyCompromise,
            "3" => RevocationReason.AffiliationChanged,
            "4" => RevocationReason.Superseded,
            "5" => RevocationReason.CessationOfOperation,
            _ => RevocationReason.Unspecified
        };
    }

    private static string MapRevocationReasonToAdminCode(RevocationReason reason) {
        return reason switch {
            RevocationReason.KeyCompromise => "1",
            RevocationReason.AffiliationChanged => "3",
            RevocationReason.Superseded => "4",
            RevocationReason.CessationOfOperation => "5",
            _ => "0"
        };
    }

    /// <inheritdoc />
    public void Dispose() {
        (_ownedLegacyClient as IDisposable)?.Dispose();
        _ownedAdminClient?.Dispose();
    }

    /// <summary>
    /// Removes a certificate by identifier using the active API configuration.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to remove.</param>
    /// <param name="reasonCode">Optional revocation reason used when calling the Admin API.</param>
    /// <param name="reason">Optional revocation reason text used when calling the Admin API.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task RemoveAsync(
        int certificateId,
        RevocationReason reasonCode = RevocationReason.Unspecified,
        string? reason = null,
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        if (_adminClient is not null) {
            var code = MapRevocationReasonToAdminCode(reasonCode);
            await _adminClient.RevokeByIdAsync(certificateId, code, reason, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        if (_legacyClient is not null) {
            await _legacyClient.DeleteAsync(certificateId, cancellationToken)
                .ConfigureAwait(false);
            return;
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    /// <summary>
    /// Renews a certificate by identifier and returns the new certificate identifier.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to renew.</param>
    /// <param name="request">Renewal request payload.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<int> RenewByIdAsync(
        int certificateId,
        RenewCertificateRequest request,
        CancellationToken cancellationToken = default) {
        Guard.AgainstNull(request, nameof(request));

        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        if (_adminClient is not null) {
            return await _adminClient
                .RenewByIdAsync(certificateId, request, cancellationToken)
                .ConfigureAwait(false);
        }

        if (_legacyClient is not null) {
            return await _legacyClient
                .RenewAsync(certificateId, request, cancellationToken)
                .ConfigureAwait(false);
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }
}
