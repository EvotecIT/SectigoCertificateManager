using SectigoCertificateManager.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
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
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(dir, Path.GetRandomFileName());
        try {
            CertificateExport.SavePem(cert, path);
            Assert.True(Directory.Exists(dir));
            Assert.True(File.Exists(path));
            var content = File.ReadAllText(path);
            Assert.Contains("BEGIN CERTIFICATE", content);
        } finally {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
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
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(dir, Path.GetRandomFileName());
        try {
            CertificateExport.SaveDer(cert, path);
            Assert.True(Directory.Exists(dir));
            Assert.True(File.Exists(path));
            var bytes = File.ReadAllBytes(path);
            Assert.Equal(cert.RawData, bytes);
        } finally {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }
    }

    [Fact]
    public void SavePfx_WritesFile() {
        using var cert = CreateCertificate();
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(dir, Path.GetRandomFileName());
        try {
            CertificateExport.SavePfx(cert, path, "pwd");
            Assert.True(Directory.Exists(dir));
            Assert.True(File.Exists(path));
            // X509Certificate2 constructor is obsolete on .NET 9.0 and later.
#pragma warning disable SYSLIB0057
            using var loaded = new X509Certificate2(path, "pwd");
#pragma warning restore SYSLIB0057
            Assert.Equal(cert.Thumbprint, loaded.Thumbprint);
        } finally {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
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
    public void SavePfx_NoDirectory_WritesFile() {
        using var cert = CreateCertificate();
        var path = Path.GetRandomFileName();
        try {
            CertificateExport.SavePfx(cert, path, "pwd");
            Assert.True(File.Exists(path));
        } finally {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void SavePfxForTest_ClearsBuffer() {
        using var cert = CreateCertificate();
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(dir, Path.GetRandomFileName());
        try {
            var buffer = CertificateExport.SavePfxForTest(cert, path, "pwd");
            Assert.True(Directory.Exists(dir));
            Assert.All(buffer, b => Assert.Equal(0, b));
        } finally {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }
    }

    [Fact]
    public void SavePfxForTest_NoDirectory_WritesFile() {
        using var cert = CreateCertificate();
        var path = Path.GetRandomFileName();
        try {
            var buffer = CertificateExport.SavePfxForTest(cert, path, "pwd");
            Assert.True(File.Exists(path));
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

        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(dir, Path.GetRandomFileName());
        try {
            CertificateExport.SavePemChain(leafCert, path, new[] { rootCert });
            Assert.True(Directory.Exists(dir));
            var content = File.ReadAllText(path);
            Assert.Contains("BEGIN CERTIFICATE", content);
            Assert.Equal(2, content.Split("-----END CERTIFICATE-----").Length - 1);
        } finally {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }
    }

    [Fact]
    public void SavePem_UsesUtf8Encoding() {
        using var cert = CreateCertificate();
        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(dir, Path.GetRandomFileName());
        try {
            CertificateExport.SavePem(cert, path);
            var bytes = File.ReadAllBytes(path);
            var preamble = Encoding.UTF8.GetPreamble();
            Assert.True(bytes.Take(preamble.Length).SequenceEqual(preamble));
            var text = Encoding.UTF8.GetString(bytes, preamble.Length, bytes.Length - preamble.Length);
            var expected = new StringBuilder()
                .AppendLine("-----BEGIN CERTIFICATE-----")
                .AppendLine(Convert.ToBase64String(cert.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks))
                .AppendLine("-----END CERTIFICATE-----")
                .ToString();
            Assert.Equal(expected, text);
        } finally {
            if (File.Exists(path)) {
                File.Delete(path);
            }
        }
    }

    [Fact]
    public void SavePemChain_UsesUtf8Encoding() {
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

        var dir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        var path = Path.Combine(dir, Path.GetRandomFileName());
        try {
            CertificateExport.SavePemChain(leafCert, path, new[] { rootCert });
            var bytes = File.ReadAllBytes(path);
            var preamble = Encoding.UTF8.GetPreamble();
            Assert.True(bytes.Take(preamble.Length).SequenceEqual(preamble));
            var text = Encoding.UTF8.GetString(bytes, preamble.Length, bytes.Length - preamble.Length);

            using var chain = new X509Chain();
            chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
            chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
            chain.ChainPolicy.ExtraStore.Add(rootCert);
            chain.Build(leafCert);

            var builder = new StringBuilder();
            foreach (var element in chain.ChainElements) {
                builder.AppendLine("-----BEGIN CERTIFICATE-----");
                builder.AppendLine(Convert.ToBase64String(element.Certificate.Export(X509ContentType.Cert), Base64FormattingOptions.InsertLineBreaks));
                builder.AppendLine("-----END CERTIFICATE-----");
            }

            Assert.Equal(builder.ToString(), text);
        } finally {
            if (Directory.Exists(dir)) {
                Directory.Delete(dir, true);
            }
        }
    }

    [Fact]
    public void CreatePfx_ReturnsValidBytes() {
        using var original = CreateCertificate();
        var certificate = LoadCertificate(original.Export(X509ContentType.Cert));
        using var key = RSA.Create();
        var pkcs8 = original.GetRSAPrivateKey()!.ExportPkcs8PrivateKey();
        key.ImportPkcs8PrivateKey(pkcs8, out _);
        Array.Clear(pkcs8, 0, pkcs8.Length);

        var bytes = CertificateExport.CreatePfx(certificate, key, "pwd");
        using var loaded = LoadPkcs12(bytes, "pwd");

        Assert.Equal(certificate.Thumbprint, loaded.Thumbprint);
        Assert.True(loaded.HasPrivateKey);
    }

    private static X509Certificate2 LoadCertificate(byte[] data) {
#if NET9_0_OR_GREATER
        return X509CertificateLoader.LoadCertificate(data);
#else
        return new X509Certificate2(data);
#endif
    }

    private static X509Certificate2 LoadPkcs12(byte[] data, string password) {
#if NET9_0_OR_GREATER
        return X509CertificateLoader.LoadPkcs12(data, password);
#else
        return new X509Certificate2(data, password);
#endif
    }
}
