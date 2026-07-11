namespace SectigoCertificateManager;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Provides download, mutation, lifecycle, and mapping behavior for <see cref="CertificateService"/>.
/// </summary>
public sealed partial class CertificateService {
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

    private static string RemoveUsageSeparators(string value) {
        // Normalize common spelling variations (for example, "Non-Repudiation" and "Non Repudiation")
        // so they map to the same canonical token.
        return value.Replace(" ", string.Empty).Replace("-", string.Empty);
    }

    private static string? NormalizeKeyUsage(IReadOnlyList<string>? values) {
        if (values == null || values.Count == 0) {
            return null;
        }

        var normalized = new List<string>(values.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string? raw in values) {
            if (string.IsNullOrWhiteSpace(raw)) {
                continue;
            }

            string token = raw.Trim();
            string compact = RemoveUsageSeparators(token);

            string label = compact.ToUpperInvariant() switch {
                "DIGITALSIGNATURE" => "DigitalSignature",
                "NONREPUDIATION" => "NonRepudiation",
                "CONTENTCOMMITMENT" => "NonRepudiation",
                "KEYENCIPHERMENT" => "KeyEncipherment",
                "DATAENCIPHERMENT" => "DataEncipherment",
                "KEYAGREEMENT" => "KeyAgreement",
                "KEYCERTSIGN" => "CertificateSign",
                "CERTIFICATESIGN" => "CertificateSign",
                "CRLSIGN" => "CrlSign",
                "ENCIPHERONLY" => "EncipherOnly",
                "DECIPHERONLY" => "DecipherOnly",
                _ => token
            };

            if (seen.Add(label)) {
                normalized.Add(label);
            }
        }

        return normalized.Count == 0 ? null : string.Join(", ", normalized);
    }

    private static string? NormalizeCommaList(IReadOnlyList<string>? values) {
        if (values == null || values.Count == 0) {
            return null;
        }

        var normalized = new List<string>(values.Count);
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (string? raw in values) {
            if (string.IsNullOrWhiteSpace(raw)) {
                continue;
            }

            string token = raw.Trim();
            if (seen.Add(token)) {
                normalized.Add(token);
            }
        }

        return normalized.Count == 0 ? null : string.Join(", ", normalized);
    }

    private static string? BuildUsagePurposes(IReadOnlyList<string>? apiPurposes, IReadOnlyList<string>? extendedKeyUsages) {
        var purposes = new List<string>();
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        static void AddPurpose(List<string> output, HashSet<string> seenSet, string purpose) {
            if (seenSet.Add(purpose)) {
                output.Add(purpose);
            }
        }

        if (apiPurposes != null) {
            foreach (string? raw in apiPurposes) {
                if (string.IsNullOrWhiteSpace(raw)) {
                    continue;
                }

                string token = raw.Trim();
                if (TryMapPurposeFromEku(token, out string purpose)) {
                    AddPurpose(purposes, seen, purpose);
                } else {
                    AddPurpose(purposes, seen, token);
                }
            }
        }

        if (extendedKeyUsages != null) {
            foreach (string? raw in extendedKeyUsages) {
                if (string.IsNullOrWhiteSpace(raw)) {
                    continue;
                }

                if (TryMapPurposeFromEku(raw.Trim(), out string purpose)) {
                    AddPurpose(purposes, seen, purpose);
                }
            }
        }

        return purposes.Count == 0 ? null : string.Join(", ", purposes);
    }

    private static bool TryMapPurposeFromEku(string ekuValue, out string purpose) {
        if (s_usagePurposeByOid.TryGetValue(ekuValue, out purpose!)) {
            return true;
        }

        string normalized = RemoveUsageSeparators(ekuValue);
        purpose = normalized.ToUpperInvariant() switch {
            "SERVERAUTHENTICATION" => "ServerAuth",
            "CLIENTAUTHENTICATION" => "ClientAuth",
            "CODESIGNING" => "CodeSigning",
            "EMAILPROTECTION" => "EmailProtection",
            "TIMESTAMPING" => "TimeStamping",
            "OCSPSIGNING" => "OcspSigning",
            "DOCUMENTSIGNING" => "DocumentSigning",
            "ANYEXTENDEDKEYUSAGE" => "AnyEku",
            _ => string.Empty
        };

        return !string.IsNullOrEmpty(purpose);
    }

    private static CertificateStatus ParseStatus(string? statusText) {
        if (statusText == null) {
            return CertificateStatus.Any;
        }

        if (statusText.Trim().Length == 0) {
            return CertificateStatus.Any;
        }

        string normalized = statusText.Replace(" ", string.Empty);
        return Enum.TryParse<CertificateStatus>(normalized, ignoreCase: true, out var status)
            ? status
            : CertificateStatus.Any;
    }

    private static RevocationReason MapRevocationReason(string? code) {
        if (code == null) {
            return RevocationReason.Unspecified;
        }

        var value = code.Trim();
        if (value.Length == 0) {
            return RevocationReason.Unspecified;
        }

        return value switch {
            "0" => RevocationReason.Unspecified,
            "1" => RevocationReason.KeyCompromise,
            "2" => RevocationReason.CaCompromise,
            "3" => RevocationReason.AffiliationChanged,
            "4" => RevocationReason.Superseded,
            "5" => RevocationReason.CessationOfOperation,
            "6" => RevocationReason.CertificateHold,
            "8" => RevocationReason.RemoveFromCrl,
            "9" => RevocationReason.PrivilegeWithdrawn,
            "10" => RevocationReason.AaCompromise,
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
            RevocationReason.CaCompromise => "2",
            RevocationReason.AffiliationChanged => "3",
            RevocationReason.Superseded => "4",
            RevocationReason.CessationOfOperation => "5",
            RevocationReason.CertificateHold => "6",
            RevocationReason.RemoveFromCrl => "8",
            RevocationReason.PrivilegeWithdrawn => "9",
            RevocationReason.AaCompromise => "10",
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
