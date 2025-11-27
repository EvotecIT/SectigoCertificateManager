namespace SectigoCertificateManager.Models;

using SectigoCertificateManager;
using System;
using System.Collections.Generic;
using System.IO;
using System.Buffers;
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

        byte[] bytes;
        try {
            bytes = Convert.FromBase64String(data);
        } catch (FormatException) {
            throw new ValidationException(new ApiError {
                Code = ApiErrorCode.UnknownError,
                Description = "Certificate data is not valid Base64."
            });
        }

        // X509Certificate2 constructor is obsolete beginning with .NET 9.0,
        // but remains necessary for earlier target frameworks.
#pragma warning disable SYSLIB0057
        return new X509Certificate2(bytes);
#pragma warning restore SYSLIB0057
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

        using var reader = new StreamReader(stream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        var rented = ArrayPool<char>.Shared.Rent(4096);
        var vsb = new ValueStringBuilder(stackalloc char[256]);
        long read = 0;
        long total = stream.CanSeek ? stream.Length : -1;

        int count;
        while ((count = reader.Read(rented, 0, rented.Length)) > 0) {
            vsb.Append(rented.AsSpan(0, count));
            read += count;
            if (progress is not null && total > 0) {
                progress.Report((double)read / total);
            }
        }

        if (progress is not null && total > 0) {
            progress.Report(1d);
        }
        var result = FromBase64(vsb.ToString());
        vsb.Dispose();
        ArrayPool<char>.Shared.Return(rented);
        return result;
    }
}
