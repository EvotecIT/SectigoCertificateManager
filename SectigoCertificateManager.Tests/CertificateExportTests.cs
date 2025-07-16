using SectigoCertificateManager.Utilities;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="CertificateExport"/> helpers.
/// </summary>
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

    /// <summary>Exports certificate as PEM file.</summary>
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
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SavePem_InvalidPath_Throws(string? path) {
        using var cert = CreateCertificate();
        Assert.Throws<ArgumentException>(() => CertificateExport.SavePem(cert, path!));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SaveDer_InvalidPath_Throws(string? path) {
        using var cert = CreateCertificate();
        Assert.Throws<ArgumentException>(() => CertificateExport.SaveDer(cert, path!));
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
            if (File.Exists(path)) {
                File.Delete(path);
            }
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
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void SavePfx_InvalidPath_Throws(string? path) {
        using var cert = CreateCertificate();
        Assert.Throws<ArgumentException>(() => CertificateExport.SavePfx(cert, path!, null));
    }

    [Fact]
    public void SavePfxForTest_ClearsBuffer() {
        using var cert = CreateCertificate();
        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            var buffer = CertificateExport.SavePfxForTest(cert, path, "pwd");
            Assert.All(buffer, b => Assert.Equal(0, b));
        } finally {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void SavePemChain_WritesChain() {
        using var rootKey = RSA.Create(2048);
#if NET472
        var rootRequest = new CertificateRequest(
            new X500DistinguishedName("CN=Root"),
            rootKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
#else
        var rootRequest = new CertificateRequest("CN=Root", rootKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#endif
        rootRequest.CertificateExtensions.Add(new X509BasicConstraintsExtension(true, false, 0, true));
        var now = DateTimeOffset.UtcNow;
        using var rootCert = rootRequest.CreateSelfSigned(now.AddDays(-1), now.AddDays(2));

        using var leafKey = RSA.Create(2048);
#if NET472
        var leafRequest = new CertificateRequest(
            new X500DistinguishedName("CN=Leaf"),
            leafKey,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
#else
        var leafRequest = new CertificateRequest("CN=Leaf", leafKey, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#endif
        var serial = new byte[8];
        RandomNumberGenerator.Fill(serial);
        var leafCert = leafRequest.Create(rootCert, now.AddDays(-1), now.AddDays(1), serial);

        var path = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        try {
            CertificateExport.SavePemChain(leafCert, path, new[] { rootCert });
            var content = File.ReadAllText(path);
            Assert.Contains("BEGIN CERTIFICATE", content);
            Assert.Equal(2, content.Split("-----END CERTIFICATE-----").Length - 1);
        } finally {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }
}