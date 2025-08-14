namespace SectigoCertificateManager.Clients;

using SectigoCertificateManager.Models;
using SectigoCertificateManager.Utilities;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

public sealed partial class CertificatesClient : BaseClient {
    /// <summary>
    /// Downloads an issued certificate and returns an <see cref="X509Certificate2"/> instance.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to download.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<X509Certificate2> DownloadAsync(
        int certificateId,
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        var endpoint = $"ssl/v1/collect/{certificateId}";
        var url = $"{endpoint}?format=base64";
        var response = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);
#if NETSTANDARD2_0 || NET472
        var base64 = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var base64 = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
        return Certificate.FromBase64(base64);
    }

    /// <summary>
    /// Downloads an issued certificate and returns the DER encoded bytes.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to download.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<byte[]> DownloadBytesAsync(
        int certificateId,
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        var endpoint = $"ssl/v1/collect/{certificateId}";
        var url = $"{endpoint}?format=base64";
        var response = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);
#if NETSTANDARD2_0 || NET472
        var base64 = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
#else
        var base64 = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
#endif
        return Convert.FromBase64String(base64);
    }

    /// <summary>
    /// Downloads an issued certificate and returns a <see cref="Stream"/> containing the DER bytes.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to download.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task<Stream> DownloadStreamAsync(
        int certificateId,
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }

        var endpoint = $"ssl/v1/collect/{certificateId}";
        var url = $"{endpoint}?format=base64";
        var response = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);
#if NETSTANDARD2_0 || NET472
        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
#else
        var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#endif
        var transform = new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces);
        return new CryptoStream(stream, transform, CryptoStreamMode.Read);
    }

    /// <summary>
    /// Downloads an issued certificate and saves it to disk.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate to download.</param>
    /// <param name="path">Destination file path.</param>
    /// <param name="format">Certificate format to request. Defaults to <c>base64</c>.</param>
    /// <param name="force">If <c>true</c>, bypasses the local cache and downloads the certificate.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task DownloadAsync(
        int certificateId,
        string path,
        string format = "base64",
        bool force = false,
        IProgress<double>? progress = null,
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }
        if (string.IsNullOrEmpty(path)) {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        if (_client.EnableDownloadCache && !force && File.Exists(path)) {
            string? localHash = null;
            if (string.Equals(format, "base64", StringComparison.OrdinalIgnoreCase)) {
#if NETSTANDARD2_0 || NET472
                var text = File.ReadAllText(path);
#else
                var text = await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
#endif
                byte[] data;
                try {
                    data = Convert.FromBase64String(text);
                } catch (FormatException) {
                    data = Array.Empty<byte>();
                }

                using var sha1 = SHA1.Create();
#if NETSTANDARD2_0 || NET472
                localHash = BitConverter.ToString(sha1.ComputeHash(data)).Replace("-", string.Empty);
#else
                localHash = Convert.ToHexString(sha1.ComputeHash(data));
#endif
            } else {
                using var fs = File.OpenRead(path);
                using var sha1 = SHA1.Create();
#if NETSTANDARD2_0 || NET472
                localHash = BitConverter.ToString(sha1.ComputeHash(fs)).Replace("-", string.Empty);
#else
                var hash = await sha1.ComputeHashAsync(fs, cancellationToken).ConfigureAwait(false);
                localHash = Convert.ToHexString(hash);
#endif
            }

            var meta = await GetAsync(certificateId, cancellationToken).ConfigureAwait(false);
            if (localHash is not null && meta?.Sha1Hash is not null &&
                meta.Sha1Hash.Equals(localHash, StringComparison.OrdinalIgnoreCase)) {
                return;
            }
        }

        var endpoint = $"ssl/v1/collect/{certificateId}";
        var url = $"{endpoint}?format={Uri.EscapeDataString(format)}";
        var response = await _client.GetAsync(url, cancellationToken).ConfigureAwait(false);
        var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
        using (stream) {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            using var file = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            var buffer = new byte[65536];
            long total = response.Content.Headers.ContentLength ?? (stream.CanSeek ? stream.Length : -1);
            long copied = 0;
            int count;
            while (true) {
#if NETSTANDARD2_0 || NET472
                count = await stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
#else
                count = await stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
#endif
                if (count == 0) {
                    break;
                }
#if NETSTANDARD2_0 || NET472
                await file.WriteAsync(buffer, 0, count).ConfigureAwait(false);
#else
                await file.WriteAsync(buffer.AsMemory(0, count), cancellationToken).ConfigureAwait(false);
#endif
                copied += count;
                if (progress is not null && total > 0) {
                    progress.Report((double)copied / total);
                }
            }

            if (progress is not null && total > 0) {
                progress.Report(1d);
            }
        }
    }

    /// <summary>
    /// Downloads the issuing certificate chain and saves it to disk.
    /// </summary>
    /// <param name="certificateId">Identifier of the certificate.</param>
    /// <param name="path">Destination file path.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    public async Task GetCaChainAsync(
        int certificateId,
        string path,
        CancellationToken cancellationToken = default) {
        if (certificateId <= 0) {
            throw new ArgumentOutOfRangeException(nameof(certificateId));
        }
        if (string.IsNullOrEmpty(path)) {
            throw new ArgumentException("Path cannot be null or empty.", nameof(path));
        }

        await DownloadAsync(certificateId, path, format: "x509IO", cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}
