namespace SectigoCertificateManager.Models;

using SectigoCertificateManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Represents a certificate returned by the Sectigo API.
/// </summary>
public sealed class Certificate {
    /// <summary>Gets or sets the identifier of the certificate.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the common name of the certificate.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the certificate status.</summary>
    public CertificateStatus Status { get; set; } = CertificateStatus.Any;

    /// <summary>Gets or sets the associated order number.</summary>
    public long OrderNumber { get; set; }

    /// <summary>Gets or sets the backend certificate identifier.</summary>
    public string BackendCertId { get; set; } = string.Empty;

    /// <summary>Gets or sets the certificate vendor.</summary>
    public string? Vendor { get; set; }

    /// <summary>Gets or sets the certificate profile.</summary>
    public Profile? CertType { get; set; }

    /// <summary>Gets or sets the certificate term.</summary>
    public int Term { get; set; }

    /// <summary>Gets or sets the owner of the certificate.</summary>
    public string? Owner { get; set; }

    /// <summary>Gets or sets the requester of the certificate.</summary>
    public string? Requester { get; set; }

    /// <summary>
    /// Gets or sets the external requester when the certificate was requested on behalf of
    /// an external party (Admin API only).
    /// </summary>
    public string? ExternalRequester { get; set; }

    /// <summary>Gets or sets additional comments.</summary>
    public string? Comments { get; set; }

    /// <summary>Gets or sets the request date.</summary>
    public string? Requested { get; set; }

    /// <summary>Gets or sets the expiry date.</summary>
    public string? Expires { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this certificate was populated using a fallback path
    /// (for example, when Admin detail operations are not available and only summary list data is used).
    /// </summary>
    public bool IsAdminDetailFallback { get; set; }

    /// <summary>
    /// Gets or sets the error message associated with a failed Admin detail retrieval, when
    /// <see cref="IsAdminDetailFallback"/> is <c>true</c>.
    /// </summary>
    public string? AdminDetailError { get; set; }

    /// <summary>Gets or sets the serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the key algorithm.</summary>
    public string? KeyAlgorithm { get; set; }

    /// <summary>Gets or sets the key size.</summary>
    public int? KeySize { get; set; }

    /// <summary>Gets or sets the key type.</summary>
    public string? KeyType { get; set; }

    /// <summary>Gets or sets normalized key usage values.</summary>
    public string? KeyUsage { get; set; }

    /// <summary>Gets or sets normalized extended key usage values.</summary>
    public string? ExtendedKeyUsage { get; set; }

    /// <summary>Gets or sets normalized usage purposes derived from EKU OIDs.</summary>
    public string? UsagePurposes { get; set; }

    /// <summary>Gets or sets the revocation timestamp text returned by API.</summary>
    public string? Revoked { get; set; }

    /// <summary>Gets or sets the revocation reason code returned by API.</summary>
    public string? RevocationReasonCode { get; set; }

    /// <summary>Gets or sets subject alternative names.</summary>
    public IReadOnlyList<string> SubjectAlternativeNames { get; set; } = [];

    /// <summary>Gets or sets a value indicating whether notifications are suspended.</summary>
    public bool SuspendNotifications { get; set; }

    /// <summary>
    /// Creates an <see cref="X509Certificate2"/> from a base64 encoded certificate.
    /// </summary>
    /// <param name="data">Base64 encoded certificate bytes.</param>
    public static X509Certificate2 FromBase64(string data) {
        if (string.IsNullOrWhiteSpace(data)) {
            throw new ArgumentException("Value cannot be null or empty.", nameof(data));
        }

        string input = data.Trim();

        if (TryCreateFromPem(input, out var pemCertificate) && pemCertificate is not null) {
            return pemCertificate;
        }

        if (TryCreateFromBase64(input, out var base64Certificate) && base64Certificate is not null) {
            return base64Certificate;
        }

        throw new ValidationException(new ApiError {
            Code = ApiErrorCode.UnknownError,
            Description = "Certificate data is not valid Base64."
        });
    }

    /// <summary>
    /// Creates an <see cref="X509Certificate2"/> from a stream containing base64 encoded data.
    /// </summary>
    /// <param name="stream">Stream providing base64 encoded certificate bytes.</param>
    /// <param name="progress">Optional progress reporter.</param>
    public static X509Certificate2 FromBase64(Stream stream, IProgress<double>? progress = null) {
        Guard.AgainstNull(stream, nameof(stream));

        if (stream.CanSeek) {
            stream.Seek(0, SeekOrigin.Begin);
        }

        var rented = ArrayPool<byte>.Shared.Rent(8192);
        try {
            using var buffer = new MemoryStream();
            long read = 0;
            long total = stream.CanSeek ? stream.Length : -1;

            int count;
            while ((count = stream.Read(rented, 0, rented.Length)) > 0) {
                buffer.Write(rented, 0, count);
                read += count;
                if (progress is not null && total > 0) {
                    progress.Report((double)read / total);
                }
            }

            if (progress is not null && total > 0) {
                progress.Report(1d);
            }

            byte[] payload = buffer.ToArray();

            if (TryCreateFromRawBytes(payload, out var rawCertificate) && rawCertificate is not null) {
                return rawCertificate;
            }

            var text = Encoding.UTF8.GetString(payload);
            return FromBase64(text);
        }
        finally {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    private static bool TryCreateFromPem(string input, out X509Certificate2? certificate) {
        certificate = null;
        const string certBeginMarker = "-----BEGIN CERTIFICATE-----";
        const string certEndMarker = "-----END CERTIFICATE-----";

        int begin = input.IndexOf(certBeginMarker, StringComparison.OrdinalIgnoreCase);
        if (begin < 0) {
            // Some Sectigo collect responses return a PKCS#7 chain instead of a single certificate PEM.
            const string pkcs7BeginMarker = "-----BEGIN PKCS7-----";
            const string pkcs7EndMarker = "-----END PKCS7-----";
            int pkcs7Begin = input.IndexOf(pkcs7BeginMarker, StringComparison.OrdinalIgnoreCase);
            if (pkcs7Begin < 0) {
                return false;
            }

            int pkcs7ContentStart = pkcs7Begin + pkcs7BeginMarker.Length;
            int pkcs7End = input.IndexOf(pkcs7EndMarker, pkcs7ContentStart, StringComparison.OrdinalIgnoreCase);
            if (pkcs7End < 0) {
                return false;
            }

            string pkcs7Base64 = input.Substring(pkcs7ContentStart, pkcs7End - pkcs7ContentStart);
            string normalizedPkcs7 = RemoveWhitespace(pkcs7Base64);
            if (string.IsNullOrWhiteSpace(normalizedPkcs7)) {
                return false;
            }

            try {
                var pkcs7Bytes = Convert.FromBase64String(normalizedPkcs7);
                return TryCreateFromPkcs7(pkcs7Bytes, out certificate);
            }
            catch (FormatException) {
                return false;
            }
        }

        int contentStart = begin + certBeginMarker.Length;
        int end = input.IndexOf(certEndMarker, contentStart, StringComparison.OrdinalIgnoreCase);
        if (end < 0) {
            return false;
        }

        string base64 = input.Substring(contentStart, end - contentStart);
        return TryCreateFromBase64(base64, out certificate);
    }

    private static bool TryCreateFromBase64(string input, out X509Certificate2? certificate) {
        certificate = null;
        string normalized = RemoveWhitespace(input);
        if (string.IsNullOrWhiteSpace(normalized)) {
            return false;
        }

        byte[] bytes;
        try {
            bytes = Convert.FromBase64String(normalized);
        }
        catch (FormatException) {
            return false;
        }

        return TryCreateFromRawBytes(bytes, out certificate);
    }

    private static bool TryCreateFromRawBytes(byte[] payload, out X509Certificate2? certificate) {
        certificate = null;
        if (payload.Length == 0) {
            return false;
        }

        try {
            #if NET9_0_OR_GREATER
            certificate = X509CertificateLoader.LoadCertificate(payload);
            #else
            certificate = new X509Certificate2(payload);
            #endif
            return true;
        }
        catch {
            return TryCreateFromPkcs7(payload, out certificate);
        }
    }

    private static bool TryCreateFromPkcs7(byte[] payload, out X509Certificate2? certificate) {
        certificate = null;
        if (payload.Length == 0) {
            return false;
        }

        try {
            var cms = new SignedCms();
            cms.Decode(payload);
            X509Certificate2Collection collection = cms.Certificates;
            if (collection.Count == 0) {
                return false;
            }

            foreach (var candidate in collection) {
                var basicConstraints = candidate.Extensions.OfType<X509BasicConstraintsExtension>().FirstOrDefault();
                if (basicConstraints is null || !basicConstraints.CertificateAuthority) {
                    certificate = candidate;
                    return true;
                }
            }

            foreach (var candidate in collection) {
                if (!candidate.Issuer.Equals(candidate.Subject, StringComparison.OrdinalIgnoreCase)) {
                    certificate = candidate;
                    return true;
                }
            }

            certificate = collection[0];
            return true;
        }
        catch {
            return false;
        }
    }

    private static string RemoveWhitespace(string input) {
        if (string.IsNullOrWhiteSpace(input)) {
            return string.Empty;
        }

        var vsb = new ValueStringBuilder(stackalloc char[256]);
        foreach (char ch in input) {
            if (!char.IsWhiteSpace(ch)) {
                vsb.Append(ch);
            }
        }

        var result = vsb.ToString();
        vsb.Dispose();
        return result;
    }
}
