using SectigoCertificateManager.Utilities;
using System;
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
}
