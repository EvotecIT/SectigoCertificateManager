using SectigoCertificateManager.Utilities;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SectigoCertificateManager.Tests;

public sealed class CertificateExportTests {
    private static X509Certificate2 CreateCertificate() {
        using var key = RSA.Create(2048);
#if NET472
        var request = new CertificateRequest(
            new X500DistinguishedName("CN=Test"),
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
#else
        var request = new CertificateRequest("CN=Test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#endif
        return request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
    }

    [Fact]
    public void SavePem_WritesFile() {
        using var cert = CreateCertificate();
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            CertificateExport.SavePem(cert, path);
            Assert.True(File.Exists(path));
            var content = File.ReadAllText(path);
            Assert.Contains("BEGIN CERTIFICATE", content);
        } finally {
            File.Delete(path);
        }
    }

    [Fact]
    public void SaveDer_WritesFile() {
        using var cert = CreateCertificate();
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            CertificateExport.SaveDer(cert, path);
            Assert.True(File.Exists(path));
            var bytes = File.ReadAllBytes(path);
            Assert.Equal(cert.RawData, bytes);
        } finally {
            File.Delete(path);
        }
    }

    [Fact]
    public void SavePfx_WritesFile() {
        using var cert = CreateCertificate();
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            CertificateExport.SavePfx(cert, path, "pwd");
            Assert.True(File.Exists(path));
            using var loaded = new X509Certificate2(path, "pwd");
            Assert.Equal(cert.Thumbprint, loaded.Thumbprint);
        } finally {
            File.Delete(path);
        }
    }

    [Fact]
    public void SavePfxForTest_ClearsBuffer() {
        using var cert = CreateCertificate();
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            var buffer = CertificateExport.SavePfxForTest(cert, path, "pwd");
            Assert.All(buffer, b => Assert.Equal(0, b));
        } finally {
            File.Delete(path);
        }
    }
}
