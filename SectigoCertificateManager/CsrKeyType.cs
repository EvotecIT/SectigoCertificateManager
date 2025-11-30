namespace SectigoCertificateManager;

/// <summary>
/// Supported key algorithms for CSR generation.
/// </summary>
public enum CsrKeyType {
    /// <summary>RSA key pair.</summary>
    Rsa = 0,

    /// <summary>Elliptic Curve key pair (ECDSA).</summary>
    Ecdsa = 1
}
