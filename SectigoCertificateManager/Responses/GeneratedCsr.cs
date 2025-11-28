namespace SectigoCertificateManager.Responses;

/// <summary>
/// CSR material returned by <c>CsrGenerator</c>.
/// </summary>
public sealed class GeneratedCsr {
    /// <summary>PEM-encoded certificate signing request.</summary>
    public string Csr { get; set; } = string.Empty;

    /// <summary>PEM-encoded PKCS#8 private key.</summary>
    public string PrivateKeyPem { get; set; } = string.Empty;

    /// <summary>PEM-encoded public key.</summary>
    public string PublicKeyPem { get; set; } = string.Empty;

    /// <summary>Key algorithm used.</summary>
    public CsrKeyType KeyType { get; set; }
        = CsrKeyType.Rsa;
}
