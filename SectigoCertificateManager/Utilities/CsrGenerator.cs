#if NETSTANDARD2_0
namespace SectigoCertificateManager.Utilities;

using System.Security.Cryptography;

/// <summary>
/// Provides helpers for generating certificate signing requests.
/// </summary>
public static class CsrGenerator {
    /// <summary>
    /// Generates an RSA-based CSR (not supported on NETSTANDARD2_0; will throw).
    /// </summary>
    /// <param name="subjectName">Distinguished name for the certificate subject.</param>
    /// <param name="keySize">RSA key size in bits.</param>
    /// <param name="hashAlgorithm">Optional hash algorithm.</param>
    public static (string Csr, RSA Key) GenerateRsa(
        string subjectName,
        int keySize = 2048,
        HashAlgorithmName? hashAlgorithm = null) => throw new PlatformNotSupportedException();

    /// <summary>
    /// Generates an ECDSA-based CSR (not supported on NETSTANDARD2_0; will throw).
    /// </summary>
    /// <param name="subjectName">Distinguished name for the certificate subject.</param>
    /// <param name="curve">Optional EC curve; defaults to P-256 when supported.</param>
    /// <param name="hashAlgorithm">Optional hash algorithm.</param>
    public static (string Csr, ECDsa Key) GenerateEcdsa(
        string subjectName,
        ECCurve? curve = null,
        HashAlgorithmName? hashAlgorithm = null) => throw new PlatformNotSupportedException();
}
#else
namespace SectigoCertificateManager.Utilities;

using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Provides helpers for generating certificate signing requests.
/// </summary>
public static class CsrGenerator {
    /// <summary>
    /// Generates an RSA-based CSR and returns both the CSR (Base64) and the RSA key.
    /// </summary>
    /// <param name="subjectName">Distinguished name for the certificate subject (for example, CN=example.com).</param>
    /// <param name="keySize">RSA key size in bits (default 2048).</param>
    /// <param name="hashAlgorithm">Optional hash algorithm; defaults to SHA256.</param>
    /// <returns>Tuple containing the CSR string and the generated RSA key.</returns>
    public static (string Csr, RSA Key) GenerateRsa(
        string subjectName,
        int keySize = 2048,
        HashAlgorithmName? hashAlgorithm = null) {
        Guard.AgainstNullOrWhiteSpace(subjectName, nameof(subjectName));
        var rsa = RSA.Create(keySize);
        var request = CreateRequest(subjectName, rsa, hashAlgorithm ?? HashAlgorithmName.SHA256);
        var csr = Convert.ToBase64String(request.CreateSigningRequest());
        return (csr, rsa);
    }

    /// <summary>
    /// Generates an ECDSA-based CSR and returns both the CSR (Base64) and the ECDSA key.
    /// </summary>
    /// <param name="subjectName">Distinguished name for the certificate subject (for example, CN=example.com).</param>
    /// <param name="curve">Elliptic curve to use; defaults to NIST P-256.</param>
    /// <param name="hashAlgorithm">Optional hash algorithm; defaults to SHA256.</param>
    /// <returns>Tuple containing the CSR string and the generated ECDSA key.</returns>
    public static (string Csr, ECDsa Key) GenerateEcdsa(
        string subjectName,
        ECCurve? curve = null,
        HashAlgorithmName? hashAlgorithm = null) {
        Guard.AgainstNullOrWhiteSpace(subjectName, nameof(subjectName));
        var ecdsa = ECDsa.Create(curve ?? ECCurve.NamedCurves.nistP256);
        var request = CreateRequest(subjectName, ecdsa, hashAlgorithm ?? HashAlgorithmName.SHA256);
        var csr = Convert.ToBase64String(request.CreateSigningRequest());
        return (csr, ecdsa);
    }

    private static CertificateRequest CreateRequest(
        string subjectName,
        AsymmetricAlgorithm key,
        HashAlgorithmName hashAlgorithm) {
#if NET472
        if (key is RSA rsa) {
            return new CertificateRequest(new X500DistinguishedName(subjectName), rsa, hashAlgorithm, RSASignaturePadding.Pkcs1);
        }
        if (key is ECDsa ecdsa) {
            return new CertificateRequest(new X500DistinguishedName(subjectName), ecdsa, hashAlgorithm);
        }
#else
        if (key is RSA rsa) {
            return new CertificateRequest(subjectName, rsa, hashAlgorithm, RSASignaturePadding.Pkcs1);
        }
        if (key is ECDsa ecdsa) {
            return new CertificateRequest(subjectName, ecdsa, hashAlgorithm);
        }
#endif
        throw new NotSupportedException($"Unsupported key type: {key.GetType().Name}");
    }
}
#endif
