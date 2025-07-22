using SectigoCertificateManager.Utilities;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates creating a PFX file from a certificate and private key.
/// </summary>
public static class CombineCertKeyToPfxExample {
    /// <summary>Executes the example.</summary>
    public static void Run() {
        using var key = RSA.Create(2048);
#if NET472
        var request = new CertificateRequest(
            new X500DistinguishedName("CN=example.com"),
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
#else
        var request = new CertificateRequest("CN=example.com", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#endif
        using var certWithKey = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
        using var certificate = new X509Certificate2(certWithKey.Export(X509ContentType.Cert));

        var pfx = CertificateExport.CreatePfx(certificate, key);
        Console.WriteLine($"Generated PFX with {pfx.Length} bytes");
    }
}

