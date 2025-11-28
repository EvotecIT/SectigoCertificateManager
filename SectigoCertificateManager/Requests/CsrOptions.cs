namespace SectigoCertificateManager.Requests;

using System.Collections.Generic;

/// <summary>
/// Options for generating a certificate signing request.
/// </summary>
public sealed class CsrOptions {
    /// <summary>Common Name (CN) component of the subject. Required.</summary>
    public string CommonName { get; set; } = string.Empty;

    /// <summary>Organization (O) component.</summary>
    public string? Organization { get; set; }
        = null;

    /// <summary>Organizational Unit (OU) component.</summary>
    public string? OrganizationalUnit { get; set; }
        = null;

    /// <summary>Locality / City (L) component.</summary>
    public string? Locality { get; set; }
        = null;

    /// <summary>State or Province (S or ST) component.</summary>
    public string? StateOrProvince { get; set; }
        = null;

    /// <summary>Two-letter ISO country code (C) component.</summary>
    public string? Country { get; set; }
        = null;

    /// <summary>Email address to embed in the subject (E).</summary>
    public string? EmailAddress { get; set; }
        = null;

    /// <summary>Optional DNS names for the Subject Alternative Name extension.</summary>
    public IList<string> DnsNames { get; set; } = new List<string>();

    /// <summary>Key algorithm to use. Defaults to RSA.</summary>
    public CsrKeyType KeyType { get; set; } = CsrKeyType.Rsa;

    /// <summary>RSA key size (only for RSA). Defaults to 2048.</summary>
    public int KeySize { get; set; } = 2048;

    /// <summary>Elliptic curve to use (only for ECDSA). Defaults to P-256.</summary>
    public CsrCurve Curve { get; set; } = CsrCurve.P256;

    /// <summary>Hash algorithm name (e.g., SHA256). Defaults to SHA256.</summary>
    public string HashAlgorithm { get; set; } = "SHA256";
}
