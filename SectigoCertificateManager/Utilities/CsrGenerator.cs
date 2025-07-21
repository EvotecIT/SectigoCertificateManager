#if NETSTANDARD2_0
namespace SectigoCertificateManager.Utilities;

using System.Security.Cryptography;

/// <summary>
/// Provides helpers for generating certificate signing requests.
/// </summary>
public static class CsrGenerator {
    public static (string Csr, RSA Key) GenerateRsa(
        string subjectName,
        int keySize = 2048,
        HashAlgorithmName? hashAlgorithm = null) => throw new PlatformNotSupportedException();

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
