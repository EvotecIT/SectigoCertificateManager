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

using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;

/// <summary>
/// Provides helpers for generating certificate signing requests.
/// </summary>
public static class CsrGenerator {
    /// <summary>
    /// Generates a CSR using structured options and returns PEM materials.
    /// </summary>
    /// <param name="options">CSR generation options.</param>
    /// <returns>PEM-encoded CSR and key material.</returns>
    public static GeneratedCsr Generate(CsrOptions options) {
        Guard.AgainstNull(options, nameof(options));
        Guard.AgainstNullOrWhiteSpace(options.CommonName, nameof(options.CommonName));

        var hash = new HashAlgorithmName(string.IsNullOrWhiteSpace(options.HashAlgorithm)
            ? "SHA256"
            : options.HashAlgorithm);

        var subject = BuildSubject(options);

        return options.KeyType switch {
            CsrKeyType.Rsa => GenerateRsaInternal(subject, options, hash),
            CsrKeyType.Ecdsa => GenerateEcdsaInternal(subject, options, hash),
            _ => throw new NotSupportedException($"Unsupported key type: {options.KeyType}")
        };
    }

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

    private static GeneratedCsr GenerateRsaInternal(string subject, CsrOptions options, HashAlgorithmName hash) {
        var rsa = RSA.Create(options.KeySize);
        try {
            var request = CreateRequest(subject, rsa, hash);
            AddSubjectAltNames(request, options.DnsNames);
            var csrBytes = request.CreateSigningRequest();
            var csrPem = ToPem("CERTIFICATE REQUEST", csrBytes);
#if NETSTANDARD2_0 || NET472
            const string emptyPem = "";
            return new GeneratedCsr {
                Csr = csrPem,
                PrivateKeyPem = emptyPem,
                PublicKeyPem = emptyPem,
                KeyType = CsrKeyType.Rsa
            };
#else
            var privatePem = ToPem("PRIVATE KEY", rsa.ExportPkcs8PrivateKey());
            var publicPem = ToPem("PUBLIC KEY", rsa.ExportSubjectPublicKeyInfo());
            return new GeneratedCsr {
                Csr = csrPem,
                PrivateKeyPem = privatePem,
                PublicKeyPem = publicPem,
                KeyType = CsrKeyType.Rsa
            };
#endif
        } finally {
            rsa.Dispose();
        }
    }

    private static GeneratedCsr GenerateEcdsaInternal(string subject, CsrOptions options, HashAlgorithmName hash) {
        var curve = options.Curve switch {
            CsrCurve.P256 => ECCurve.NamedCurves.nistP256,
            CsrCurve.P384 => ECCurve.NamedCurves.nistP384,
            CsrCurve.P521 => ECCurve.NamedCurves.nistP521,
            _ => ECCurve.NamedCurves.nistP256
        };

        using var ecdsa = ECDsa.Create(curve);
        var request = CreateRequest(subject, ecdsa, hash);
        AddSubjectAltNames(request, options.DnsNames);
        var csrBytes = request.CreateSigningRequest();
        var csrPem = ToPem("CERTIFICATE REQUEST", csrBytes);
#if NETSTANDARD2_0 || NET472
        const string emptyPem = "";
        return new GeneratedCsr {
            Csr = csrPem,
            PrivateKeyPem = emptyPem,
            PublicKeyPem = emptyPem,
            KeyType = CsrKeyType.Ecdsa
        };
#else
        var privatePem = ToPem("PRIVATE KEY", ecdsa.ExportPkcs8PrivateKey());
        var publicPem = ToPem("PUBLIC KEY", ecdsa.ExportSubjectPublicKeyInfo());
        return new GeneratedCsr {
            Csr = csrPem,
            PrivateKeyPem = privatePem,
            PublicKeyPem = publicPem,
            KeyType = CsrKeyType.Ecdsa
        };
#endif
    }

    private static string BuildSubject(CsrOptions options) {
        var parts = new List<string> { $"CN={options.CommonName}" };
        if (!string.IsNullOrWhiteSpace(options.Organization)) parts.Add($"O={options.Organization}");
        if (!string.IsNullOrWhiteSpace(options.OrganizationalUnit)) parts.Add($"OU={options.OrganizationalUnit}");
        if (!string.IsNullOrWhiteSpace(options.Locality)) parts.Add($"L={options.Locality}");
        if (!string.IsNullOrWhiteSpace(options.StateOrProvince)) parts.Add($"S={options.StateOrProvince}");
        if (!string.IsNullOrWhiteSpace(options.Country)) parts.Add($"C={options.Country}");
        if (!string.IsNullOrWhiteSpace(options.EmailAddress)) parts.Add($"E={options.EmailAddress}");
        return string.Join(",", parts);
    }

    private static void AddSubjectAltNames(CertificateRequest request, IEnumerable<string> dnsNames) {
        if (dnsNames == null) { return; }
        var names = dnsNames.Where(n => !string.IsNullOrWhiteSpace(n)).ToList();
        if (names.Count == 0) { return; }

        var sanBuilder = new SubjectAlternativeNameBuilder();
        foreach (var name in names) {
            sanBuilder.AddDnsName(name);
        }
        request.CertificateExtensions.Add(sanBuilder.Build());
    }

    private static string ToPem(string label, byte[] data) {
        var base64 = Convert.ToBase64String(data);
        var lines = Enumerable.Range(0, (base64.Length + 63) / 64)
            .Select(i => base64.Substring(i * 64, Math.Min(64, base64.Length - i * 64)));
        return $"-----BEGIN {label}-----\n{string.Join("\n", lines)}\n-----END {label}-----\n";
    }
}
#endif
