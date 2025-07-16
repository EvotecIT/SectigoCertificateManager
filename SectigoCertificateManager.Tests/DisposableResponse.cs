using System.Net.Http;

namespace SectigoCertificateManager.Tests;

internal sealed class DisposableResponse : HttpResponseMessage
{
    public bool Disposed { get; private set; }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        Disposed = true;
    }
}
