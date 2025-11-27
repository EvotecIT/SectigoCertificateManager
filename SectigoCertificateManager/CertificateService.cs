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
    /// <param name="status">Optional certificate status filter. For legacy API, <see cref="CertificateStatus.Any"/> means no status filter.</param>
    /// <param name="orgId">Optional Admin organization identifier filter. Ignored for legacy API.</param>
    /// <param name="requester">Optional Admin requester filter. Ignored for legacy API.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<IReadOnlyList<Certificate>> ListAsync(
        int size,
        int position = 0,
        CertificateStatus status = CertificateStatus.Any,
        int? orgId = null,
        string? requester = null,
        CancellationToken cancellationToken = default) {
        return ListAsync(size, position, status, orgId, requester, expiresBefore: null, expiresAfter: null, cancellationToken);
    }

    /// <summary>
    /// Lists certificates using the active API configuration.
    /// </summary>
    /// <param name="size">Number of certificates to retrieve.</param>
    /// <param name="position">Position offset for paging.</param>
    /// <param name="status">Optional Admin status filter (for example, Issued or Expired). Ignored for legacy API.</param>
    /// <param name="orgId">Optional Admin organization identifier filter. Ignored for legacy API.</param>
    /// <param name="requester">Optional Admin requester filter. Ignored for legacy API.</param>
    /// <param name="expiresBefore">
    /// Optional upper bound for the certificate expiration date. When specified, only certificates
    /// expiring on or before this date (inclusive) are returned when using the Admin API.
    /// Ignored for the legacy API.
    /// </param>
    /// <param name="expiresAfter">
    /// Optional lower bound for the certificate expiration date. When specified, only certificates
    /// expiring on or after this date (inclusive) are returned when using the Admin API.
    /// Ignored for the legacy API.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<Certificate>> ListAsync(
        int size,
        int position,
        CertificateStatus status,
        int? orgId,
        string? requester,
        DateTimeOffset? expiresBefore,
        DateTimeOffset? expiresAfter,
        CancellationToken cancellationToken = default) {
        if (_adminClient is not null) {
            string? statusText = null;
            if (status != CertificateStatus.Any) {
                statusText = status.ToString();
            }

            var identities = await _adminClient
                .ListAsync(size, position, statusText, orgId, requester, expiresBefore, expiresAfter, cancellationToken)
                .ConfigureAwait(false);
            var list = new List<Certificate>(identities.Count);
            foreach (var identity in identities) {
                list.Add(MapIdentity(identity));
            }
            return list;
        }

        if (_legacyClient is not null) {
            var request = new CertificateSearchRequest { Size = size, Position = position };
            if (status != CertificateStatus.Any) {
                request.Status = status;
            }
            var response = await _legacyClient
                .SearchAsync(request, cancellationToken)
                .ConfigureAwait(false);
            return response?.Certificates ?? Array.Empty<Certificate>();
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    /// <summary>
    /// Lists certificates using the active API configuration, retrieving full details
    /// for each certificate when using the Admin Operations API.
    /// </summary>
    /// <param name="size">Number of certificates to retrieve.</param>
    /// <param name="position">Position offset for paging.</param>
    /// <param name="status">Optional Admin status filter (for example, Issued or Expired). Ignored for legacy API.</param>
    /// <param name="orgId">Optional Admin organization identifier filter. Ignored for legacy API.</param>
    /// <param name="requester">Optional Admin requester filter. Ignored for legacy API.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    /// When configured with the legacy API this method behaves the same as <see cref="ListAsync(int,int,CertificateStatus,int?,string?,System.Threading.CancellationToken)"/>,
    /// because the legacy search endpoint already returns detailed certificate objects.
    /// </remarks>
    public Task<IReadOnlyList<Certificate>> ListDetailedAsync(
        int size,
        int position = 0,
        CertificateStatus status = CertificateStatus.Any,
        int? orgId = null,
        string? requester = null,
        CancellationToken cancellationToken = default) {
        return ListDetailedAsync(size, position, status, orgId, requester, expiresBefore: null, expiresAfter: null, cancellationToken);
    }

    /// <summary>
    /// Lists certificates using the active API configuration, retrieving full details
    /// for each certificate when using the Admin Operations API.
    /// </summary>
    /// <param name="size">Number of certificates to retrieve.</param>
    /// <param name="position">Position offset for paging.</param>
    /// <param name="status">Optional certificate status filter. For legacy API, <see cref="CertificateStatus.Any"/> means no status filter.</param>
    /// <param name="orgId">Optional Admin organization identifier filter. Ignored for legacy API.</param>
    /// <param name="requester">Optional Admin requester filter. Ignored for legacy API.</param>
    /// <param name="expiresBefore">
    /// Optional upper bound for the certificate expiration date. When specified, only certificates
    /// expiring on or before this date (inclusive) are returned when using the Admin API.
    /// Ignored for the legacy API.
    /// </param>
    /// <param name="expiresAfter">
    /// Optional lower bound for the certificate expiration date. When specified, only certificates
    /// expiring on or after this date (inclusive) are returned when using the Admin API.
    /// Ignored for the legacy API.
    /// </param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    /// When configured with the legacy API this method behaves the same as <see cref="ListAsync(int,int,CertificateStatus,int?,string?,DateTimeOffset?,DateTimeOffset?,System.Threading.CancellationToken)"/>,
    /// because the legacy search endpoint already returns detailed certificate objects.
    /// </remarks>
    public async Task<IReadOnlyList<Certificate>> ListDetailedAsync(
        int size,
        int position,
        CertificateStatus status,
        int? orgId,
        string? requester,
        DateTimeOffset? expiresBefore,
        DateTimeOffset? expiresAfter,
        CancellationToken cancellationToken = default) {
        if (_adminClient is not null) {
            string? statusText = null;
            if (status != CertificateStatus.Any) {
                statusText = status.ToString();
            }

            var identities = await _adminClient
                .ListAsync(size, position, statusText, orgId, requester, expiresBefore, expiresAfter, cancellationToken)
                .ConfigureAwait(false);
            if (identities.Count == 0) {
                return Array.Empty<Certificate>();
            }

            var list = new List<Certificate>(identities.Count);
            foreach (var identity in identities) {
                AdminSslCertificateDetails? details = null;
                string? detailError = null;
                try {
                    details = await _adminClient
                        .GetAsync(identity.SslId, cancellationToken)
                        .ConfigureAwait(false);
                } catch (ApiException ex) {
                    detailError = ex.Message;
                } catch (HttpRequestException ex) {
                    detailError = ex.Message;
                }

                var certificate = details is not null ? MapDetails(details) : MapIdentity(identity);
                if (detailError is not null) {
                    certificate.IsAdminDetailFallback = true;
                    certificate.AdminDetailError = detailError;
                }

                list.Add(certificate);
            }

            if (expiresBefore is null && expiresAfter is null) {
                return list;
            }

            var filtered = new List<Certificate>(list.Count);
            foreach (var certificate in list) {
                if (!ShouldIncludeByExpiry(certificate.Expires, expiresBefore, expiresAfter)) {
                    continue;
                }

                filtered.Add(certificate);
            }

            return filtered;
        }

        if (_legacyClient is not null) {
            return await ListAsync(size, position, status, orgId, requester, expiresBefore, expiresAfter, cancellationToken).ConfigureAwait(false);
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    /// <summary>
    /// Lists certificates that expire within the specified number of days using the Admin API.
    /// </summary>
    /// <param name="expiresWithinDays">Number of days from now within which certificates must expire.</param>
    /// <param name="status">Optional certificate status filter.</param>
    /// <param name="orgId">Optional Admin organization identifier filter.</param>
    /// <param name="requester">Optional Admin requester filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    /// This method is only supported when the service is configured with an Admin API connection.
    /// </remarks>
    public Task<IReadOnlyList<Certificate>> ListExpiringAsync(
        int expiresWithinDays,
        CertificateStatus status = CertificateStatus.Any,
        int? orgId = null,
        string? requester = null,
        CancellationToken cancellationToken = default) {
        return ListExpiringAsync(expiresWithinDays, status, orgId, requester, cancellationToken, progress: null, maxCertificatesToScan: null);
    }

    /// <summary>
    /// Lists certificates that expire within the specified number of days using the Admin API.
    /// </summary>
    /// <param name="expiresWithinDays">Number of days from now within which certificates must expire.</param>
    /// <param name="status">Optional certificate status filter.</param>
    /// <param name="orgId">Optional Admin organization identifier filter.</param>
    /// <param name="requester">Optional Admin requester filter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <param name="progress">
    /// Optional progress reporter that receives the total number of certificates processed so far.
    /// </param>
    /// <param name="maxCertificatesToScan">
    /// Optional maximum number of certificates to scan. When specified, the method stops scanning
    /// after approximately this many certificates have been processed. Intended primarily for testing.
    /// </param>
    /// <remarks>
    /// This method is only supported when the service is configured with an Admin API connection.
    /// </remarks>
    public async Task<IReadOnlyList<Certificate>> ListExpiringAsync(
        int expiresWithinDays,
        CertificateStatus status = CertificateStatus.Any,
        int? orgId = null,
        string? requester = null,
        CancellationToken cancellationToken = default,
        IProgress<int>? progress = null,
        int? maxCertificatesToScan = null) {
        if (expiresWithinDays <= 0) {
            throw new ArgumentOutOfRangeException(nameof(expiresWithinDays));
        }

        if (_adminClient is null) {
            if (_legacyClient is not null) {
                throw new InvalidOperationException("Listing expiring certificates is only supported for Admin API connections.");
            }

            throw new InvalidOperationException("No underlying client is configured for CertificateService.");
        }

        var now = DateTimeOffset.UtcNow;
        var cutoff = now.AddDays(expiresWithinDays);

        var result = new List<Certificate>();
        var position = 0;
        const int pageSize = 200;
        var processed = 0;
        int? total = null;

        // modest parallelism to keep API load reasonable
        const int maxConcurrency = 4;

        while (true) {
            string? statusText = null;
            if (status != CertificateStatus.Any) {
                statusText = status.ToString();
            }

            var listResult = await _adminClient
                .ListWithTotalAsync(pageSize, position, statusText, orgId, requester, expiresBefore: null, expiresAfter: null, cancellationToken)
                .ConfigureAwait(false);
            var identities = listResult.Items;
            if (total is null && listResult.TotalCount.HasValue && listResult.TotalCount.Value > 0) {
                total = listResult.TotalCount;
                progress?.Report(-total.Value);
            }

            if (identities.Count == 0) {
                break;
            }

            using var semaphore = new SemaphoreSlim(maxConcurrency);
            var tasks = new List<Task<Certificate?>>(identities.Count);

            foreach (var identity in identities) {
                await semaphore.WaitAsync(cancellationToken).ConfigureAwait(false);

                var idCopy = identity;
                var task = Task.Run(async () => {
                    try {
                        AdminSslCertificateDetails? details = null;
                        string? detailError = null;
                        try {
                            details = await _adminClient
                                .GetAsync(idCopy.SslId, cancellationToken)
                                .ConfigureAwait(false);
                        } catch (ApiException ex) {
                            detailError = ex.Message;
                        } catch (HttpRequestException ex) {
                            detailError = ex.Message;
                        }

                        var certificate = details is not null ? MapDetails(details) : MapIdentity(idCopy);
                        if (detailError is not null) {
                            certificate.IsAdminDetailFallback = true;
                            certificate.AdminDetailError = detailError;
                        }
                var current = Interlocked.Increment(ref processed);
                progress?.Report(current);

                        if (!ShouldIncludeByExpiry(certificate.Expires, cutoff, now)) {
                            return null;
                        }

                        return (Certificate?)certificate;
                    } finally {
                        semaphore.Release();
                    }
                }, cancellationToken);

                tasks.Add(task);
            }

            var pageResults = await Task.WhenAll(tasks).ConfigureAwait(false);
            foreach (var certificate in pageResults) {
                if (certificate is not null) {
                    result.Add(certificate);
                    if (maxCertificatesToScan is int limit && result.Count >= limit) {
                        break;
                    }
                }
            }

            if ((identities.Count < pageSize) || (maxCertificatesToScan is int outerLimit && result.Count >= outerLimit)) {
                break;
            }

            position += pageSize;
        }

        return result;
    }

    private static bool ShouldIncludeByExpiry(
        string? expiresText,
        DateTimeOffset? expiresBefore,
        DateTimeOffset? expiresAfter) {
        if (expiresBefore is null && expiresAfter is null) {
            return true;
        }

        if (string.IsNullOrWhiteSpace(expiresText)) {
            return false;
        }

        if (!DateTimeOffset.TryParse(expiresText, out var expires)) {
            return false;
        }

        if (expiresBefore is not null && expires > expiresBefore.Value) {
            return false;
        }

        if (expiresAfter is not null && expires < expiresAfter.Value) {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Retrieves available certificate types using the active API configuration.
    /// </summary>
    /// <param name="organizationId">Optional organization identifier used to filter types.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<CertificateType>> ListCertificateTypesAsync(
        int? organizationId = null,
        CancellationToken cancellationToken = default) {
        if (_adminClient is not null) {
            return await _adminClient
                .ListCertificateTypesAsync(organizationId, cancellationToken)
                .ConfigureAwait(false);
        }

        if (_legacyClient is not null) {
            var client = new CertificateTypesClient(_ownedLegacyClient ?? throw new InvalidOperationException("Legacy client wrapper is not available."));
            return await client
                .ListTypesAsync(organizationId, cancellationToken)
                .ConfigureAwait(false);
        }

        throw new InvalidOperationException("No underlying client is configured for CertificateService.");
    }

    /// <summary>
    /// Retrieves custom fields using the active API configuration.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<IReadOnlyList<CustomField>> ListCustomFieldsAsync(
        CancellationToken cancellationToken = default) {
        if (_adminClient is not null) {
            return await _adminClient
                .ListCustomFieldsAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        if (_legacyClient is not null) {
            // Legacy API exposes custom-field creation/update but not a direct "list all" endpoint.
            // For now, list custom fields is supported only for Admin connections.
            return Array.Empty<CustomField>();
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
                    .ListAsync(pageSize, position, status: null, orgId: null, requester: null, expiresBefore: null, expiresAfter: null, cancellationToken)
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

    /// <summary>
    /// Creates a keystore download link for a certificate using the Admin API.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="formatType">Keystore format type.</param>
    /// <param name="passphrase">Optional passphrase used to protect the keystore.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public Task<string> CreateKeystoreDownloadLinkAsync(
        int certificateId,
        KeystoreFormatType formatType,
        string? passphrase = null,
        CancellationToken cancellationToken = default) {
        return CreateKeystoreDownloadLinkAsync(certificateId, MapKeystoreFormat(formatType), passphrase, cancellationToken);
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

    private static string MapKeystoreFormat(KeystoreFormatType formatType) {
        return formatType switch {
            KeystoreFormatType.Key => "key",
            KeystoreFormatType.P12 => "p12",
            KeystoreFormatType.P12Aes => "p12aes",
            KeystoreFormatType.Jks => "jks",
            KeystoreFormatType.Pem => "pem",
            _ => "p12"
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
