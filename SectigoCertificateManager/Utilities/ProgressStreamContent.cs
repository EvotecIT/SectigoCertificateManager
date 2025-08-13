namespace SectigoCertificateManager.Utilities;

using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Provides an <see cref="HttpContent"/> implementation that reports upload progress.
/// </summary>
internal sealed class ProgressStreamContent : HttpContent {
    private readonly Stream _stream;
    private readonly IProgress<double>? _progress;
    private readonly int _bufferSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressStreamContent"/> class.
    /// </summary>
    /// <param name="stream">Source data stream.</param>
    /// <param name="progress">Optional progress reporter.</param>
    /// <param name="bufferSize">Buffer size used when reading the stream.</param>
    public ProgressStreamContent(Stream stream, IProgress<double>? progress = null, int bufferSize = 81920) {
        _stream = Guard.AgainstNull(stream, nameof(stream));
        _progress = progress;
        if (bufferSize <= 0) {
            throw new ArgumentOutOfRangeException(nameof(bufferSize), "Buffer size must be greater than 0.");
        }

        _bufferSize = bufferSize;
    }

#if NET5_0_OR_GREATER
    protected override async Task SerializeToStreamAsync(Stream target, TransportContext? context, CancellationToken cancellationToken) {
        await TransferToAsync(target, cancellationToken).ConfigureAwait(false);
    }
#endif
    protected override async Task SerializeToStreamAsync(Stream target, TransportContext? context) {
#if NET5_0_OR_GREATER
        await SerializeToStreamAsync(target, context, CancellationToken.None).ConfigureAwait(false);
#else
        await TransferToAsync(target, CancellationToken.None).ConfigureAwait(false);
#endif
    }

    private async Task TransferToAsync(Stream target, CancellationToken cancellationToken) {
        var buffer = new byte[_bufferSize];
        long total = _stream.CanSeek ? _stream.Length : -1;
        long uploaded = 0;
        int read;
        while (true) {
#if NETSTANDARD2_0 || NET472
            read = await _stream.ReadAsync(buffer, 0, buffer.Length).ConfigureAwait(false);
#else
            read = await _stream.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken).ConfigureAwait(false);
#endif
            if (read == 0) {
                break;
            }
#if NETSTANDARD2_0 || NET472
            await target.WriteAsync(buffer, 0, read).ConfigureAwait(false);
#else
            await target.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
#endif
            uploaded += read;
            if (_progress is not null && total > 0) {
                _progress.Report((double)uploaded / total);
            }
        }

        if (_progress is not null && total > 0) {
            _progress.Report(1d);
        }
    }

    protected override bool TryComputeLength(out long length) {
        if (_stream.CanSeek) {
            length = _stream.Length;
            return true;
        }

        length = 0;
        return false;
    }
}
