namespace SectigoCertificateManager.Utilities;

using System.Net;
using System.Net.Http;

/// <summary>
/// Delegates HTTP serialization without taking ownership of the wrapped content.
/// </summary>
internal sealed class NonDisposingHttpContent : HttpContent {
    private readonly HttpContent _inner;

    /// <summary>
    /// Initializes a wrapper that preserves the caller's ownership of <paramref name="inner"/>.
    /// </summary>
    public NonDisposingHttpContent(HttpContent inner) {
        _inner = Guard.AgainstNull(inner, nameof(inner));
        foreach (var header in inner.Headers) {
            Headers.TryAddWithoutValidation(header.Key, header.Value);
        }
    }

#if NET5_0_OR_GREATER
    protected override Task SerializeToStreamAsync(
        Stream stream,
        TransportContext? context,
        CancellationToken cancellationToken) =>
        _inner.CopyToAsync(stream, context, cancellationToken);
#endif

    protected override Task SerializeToStreamAsync(Stream stream, TransportContext? context) =>
        _inner.CopyToAsync(stream);

    protected override bool TryComputeLength(out long length) {
        if (_inner.Headers.ContentLength is long contentLength) {
            length = contentLength;
            return true;
        }

        length = 0;
        return false;
    }
}
