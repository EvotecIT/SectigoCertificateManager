namespace SectigoCertificateManager.Requests;

using System.IO;
using System.Text;

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
        if (stream is null) {
            throw new ArgumentNullException(nameof(stream));
        }

        using var reader = new StreamReader(stream, Encoding.ASCII, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true);
        var builder = new StringBuilder();
        var buffer = new char[4096];
        long read = 0;
        long total = stream.CanSeek ? stream.Length : -1;

        int count;
        while ((count = reader.Read(buffer, 0, buffer.Length)) > 0) {
            builder.Append(buffer, 0, count);
            read += count;
            if (progress is not null && total > 0) {
                progress.Report((double)read / total);
            }
        }

        if (progress is not null && total > 0) {
            progress.Report(1d);
        }

        Csr = builder.ToString();
    }
}