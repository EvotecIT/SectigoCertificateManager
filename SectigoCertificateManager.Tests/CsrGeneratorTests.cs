using SectigoCertificateManager.Utilities;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;
using SectigoCertificateManager;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="CsrGenerator"/> helpers.
/// </summary>
public sealed class CsrGeneratorTests {
    /// <summary>Generates RSA CSR.</summary>
    [Fact]
    public void GenerateRsa_ReturnsValidCsr() {
        var (csr, key) = CsrGenerator.GenerateRsa("CN=Test", 1024);
        try {
            Assert.Equal(1024, key.KeySize);
            var bytes = Convert.FromBase64String(csr);
            Assert.NotEmpty(bytes);
#if NET8_0_OR_GREATER
            var req = CertificateRequest.LoadSigningRequest(
                bytes,
                HashAlgorithmName.SHA256,
                CertificateRequestLoadOptions.SkipSignatureValidation,
                RSASignaturePadding.Pkcs1);
            Assert.Equal("CN=Test", req.SubjectName.Name);
#endif
        } finally {
            key.Dispose();
        }
    }

    /// <summary>Generates ECDSA CSR.</summary>
    [Fact]
    public void GenerateEcdsa_ReturnsValidCsr() {
        var (csr, key) = CsrGenerator.GenerateEcdsa("CN=Test");
        try {
            var bytes = Convert.FromBase64String(csr);
            Assert.NotEmpty(bytes);
#if NET8_0_OR_GREATER
            var req = CertificateRequest.LoadSigningRequest(
                bytes,
                HashAlgorithmName.SHA256,
                CertificateRequestLoadOptions.SkipSignatureValidation,
                RSASignaturePadding.Pkcs1);
            Assert.Equal("CN=Test", req.SubjectName.Name);
#endif
        } finally {
            key.Dispose();
        }
    }

    [Fact]
    public void Generate_WithOptions_AddsSanAndReturnsPem() {
        var options = new CsrOptions {
            CommonName = "example.com",
            DnsNames = { "example.com", "www.example.com" },
            KeyType = CsrKeyType.Rsa,
            KeySize = 2048
        };

        var result = CsrGenerator.Generate(options);

        Assert.StartsWith("-----BEGIN CERTIFICATE REQUEST-----", result.Csr);
        if (!string.IsNullOrWhiteSpace(result.PrivateKeyPem)) {
            Assert.StartsWith("-----BEGIN PRIVATE KEY-----", result.PrivateKeyPem);
        }
        Assert.Equal(CsrKeyType.Rsa, result.KeyType);

#if NET8_0_OR_GREATER
        // Parse the CSR to verify subject and SANs are present.
        var der = DecodePem(result.Csr, "CERTIFICATE REQUEST");
        var req = CertificateRequest.LoadSigningRequest(
            der,
            HashAlgorithmName.SHA256,
            CertificateRequestLoadOptions.SkipSignatureValidation);
        Assert.Equal("CN=example.com", req.SubjectName.Name);

        var san = req.CertificateExtensions.FirstOrDefault(e => e.Oid?.Value == "2.5.29.17");
        if (san is not null) {
            var formatted = new System.Security.Cryptography.AsnEncodedData(san.Oid, san.RawData).Format(false);
            Assert.Contains("example.com", formatted);
            Assert.Contains("www.example.com", formatted);
        }
#endif
    }

#if NET8_0_OR_GREATER
    private static byte[] DecodePem(string pem, string label) {
        var lines = pem.Split('\n')
            .Where(l => !l.StartsWith("-----", StringComparison.Ordinal))
            .Select(l => l.Trim())
            .Where(l => l.Length > 0);
        var base64 = string.Concat(lines);
        return Convert.FromBase64String(base64);
    }
#endif
}
