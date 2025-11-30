namespace SectigoCertificateManager;

/// <summary>
/// Named curves supported by CSR generation helpers.
/// </summary>
public enum CsrCurve {
    /// <summary>NIST P-256 (secp256r1).</summary>
    P256 = 0,

    /// <summary>NIST P-384 (secp384r1).</summary>
    P384 = 1,

    /// <summary>NIST P-521 (secp521r1).</summary>
    P521 = 2
}
