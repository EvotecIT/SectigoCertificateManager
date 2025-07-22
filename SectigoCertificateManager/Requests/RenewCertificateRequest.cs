namespace SectigoCertificateManager.Requests;

using System.IO;
using System.Text;
using System.Buffers;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Request payload used to renew a certificate.
/// </summary>
public sealed class RenewCertificateRequest {
    /// <summary>Gets or sets the certificate signing request.</summary>
    public string? Csr { get; set; }

    /// <summary>Gets or sets the DCV mode.</summary>
    public string? DcvMode { get; set; }

    /// <summary>Gets or sets the DCV email.</summary>
    public string? DcvEmail { get; set; }

    /// <summary>
    /// Populates <see cref="Csr"/> from a stream containing base64 encoded data.
    /// </summary>
    /// <param name="stream">Stream providing CSR bytes.</param>
    /// <param name="progress">Optional progress reporter.</param>
    public void SetCsr(Stream stream, IProgress<double>? progress = null) {
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
        Csr = vsb.ToString();
        vsb.Dispose();
        ArrayPool<char>.Shared.Return(rented);
    }
}