namespace SectigoCertificateManager.Requests;

using SectigoCertificateManager;
using SectigoCertificateManager.Utilities;
using System;
using System.Buffers;
using System.IO;
using System.Text;
using System.Text.Json.Serialization;

/// <summary>
/// Request payload used to renew a certificate.
/// </summary>
public sealed class RenewCertificateRequest {
    /// <summary>Gets or sets the certificate signing request.</summary>
    public string? Csr { get; set; }

    /// <summary>
    /// Gets or sets the DCV mode as a strongly-typed value.
    /// </summary>
    [JsonIgnore]
    public DcvMode DcvMode { get; set; }

    /// <summary>
    /// Gets or sets the DCV mode as a wire value used by the Sectigo API.
    /// </summary>
    [JsonPropertyName("dcvMode")]
    public string? DcvModeText {
        get => MapDcvMode(DcvMode);
        set => DcvMode = ParseDcvMode(value);
    }

    /// <summary>Gets or sets the DCV email.</summary>
    public string? DcvEmail { get; set; }

    /// <summary>
    /// Populates <see cref="Csr"/> from a stream containing base64 encoded data.
    /// </summary>
    /// <param name="stream">Stream providing CSR bytes.</param>
    /// <param name="progress">Optional progress reporter.</param>
    public void SetCsr(Stream stream, IProgress<double>? progress = null) {
        Guard.AgainstNull(stream, nameof(stream));

        if (!stream.CanRead) {
            throw new ArgumentException("Stream must be readable.", nameof(stream));
        }

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
        Csr = vsb.ToString();
        vsb.Dispose();
        ArrayPool<char>.Shared.Return(rented);
    }

    private static string? MapDcvMode(DcvMode mode) {
        return mode switch {
            DcvMode.None => null,
            DcvMode.Email => "EMAIL",
            DcvMode.Cname => "CNAME",
            DcvMode.Http => "HTTP",
            DcvMode.Https => "HTTPS",
            DcvMode.Txt => "TXT",
            _ => null
        };
    }

    private static DcvMode ParseDcvMode(string? value) {
        if (string.IsNullOrWhiteSpace(value)) {
            return DcvMode.None;
        }

        var upper = value.Trim().ToUpperInvariant();
        return upper switch {
            "EMAIL" => DcvMode.Email,
            "CNAME" => DcvMode.Cname,
            "HTTP" => DcvMode.Http,
            "HTTPS" => DcvMode.Https,
            "TXT" => DcvMode.Txt,
            _ => DcvMode.None
        };
    }
}
