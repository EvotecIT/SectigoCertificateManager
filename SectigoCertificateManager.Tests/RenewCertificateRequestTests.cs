using SectigoCertificateManager.Requests;
using System.IO;
using System.Text;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="RenewCertificateRequest"/> helpers.
/// </summary>
public sealed class RenewCertificateRequestTests {
    private const string Base64Csr = "VGVzdENzUg=="; // "TestCsr" in base64

    private sealed class TestProgress : IProgress<double> {
        public double Value { get; private set; }
        public void Report(double value) => Value = value;
    }

    private sealed class UnreadableStream : MemoryStream {
        public override bool CanRead => false;
    }

    /// <summary>Reads CSR from a stream.</summary>
    [Fact]
    public void SetCsr_FromStream_SetsProperty() {
        var bytes = Encoding.ASCII.GetBytes(Base64Csr);
        using var stream = new MemoryStream(bytes);
        var request = new RenewCertificateRequest();

        request.SetCsr(stream);

        Assert.Equal(Base64Csr, request.Csr);
    }

    [Fact]
    public void SetCsr_ReportsProgress() {
        var bytes = Encoding.ASCII.GetBytes(Base64Csr);
        using var stream = new MemoryStream(bytes);
        var request = new RenewCertificateRequest();
        var progress = new TestProgress();

        request.SetCsr(stream, progress);

        Assert.Equal(1d, progress.Value, 3);
    }

    [Fact]
    public void SetCsr_SeeksToBeginning() {
        var bytes = Encoding.ASCII.GetBytes(Base64Csr);
        using var stream = new MemoryStream(bytes);
        stream.Seek(2, SeekOrigin.Begin);
        var request = new RenewCertificateRequest();

        request.SetCsr(stream);

        Assert.Equal(Base64Csr, request.Csr);
    }

    [Fact]
    public void SetCsr_UnreadableStream_Throws() {
        using var stream = new UnreadableStream();
        var request = new RenewCertificateRequest();

        Assert.Throws<ArgumentException>(() => request.SetCsr(stream));
    }
}