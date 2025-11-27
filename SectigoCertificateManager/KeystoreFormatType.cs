namespace SectigoCertificateManager;

/// <summary>
/// Enumerates keystore format types supported by the Admin Operations API.
/// </summary>
public enum KeystoreFormatType {
    /// <summary>PEM encoded private key.</summary>
    Key,

    /// <summary>PKCS#12 keystore.</summary>
    P12,

    /// <summary>PKCS#12 keystore protected with AES.</summary>
    P12Aes,

    /// <summary>Java KeyStore.</summary>
    Jks,

    /// <summary>PEM encoded certificate and private key.</summary>
    Pem
}

