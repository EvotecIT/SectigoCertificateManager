namespace SectigoCertificateManager.Requests;

using System;

/// <summary>
/// Well-known encryption types for S/MIME P12 downloads in the Admin Operations API.
/// </summary>
public static class AdminSmimeP12EncryptionTypes {
    /// <summary>
    /// AES-256 with SHA-256, as defined by the Admin API.
    /// </summary>
    public const string Aes256Sha256 = "AES256-SHA256";

    /// <summary>
    /// TripleDES with SHA-1, as defined by the Admin API.
    /// </summary>
    public const string TripleDesSha1 = "TripleDES-SHA1";

    /// <summary>
    /// Returns <c>true</c> when the specified encryption type is supported by the Admin API.
    /// </summary>
    /// <param name="encryptionType">Encryption type to validate.</param>
    public static bool IsSupported(string encryptionType) {
        if (string.IsNullOrWhiteSpace(encryptionType)) {
            return false;
        }

        return string.Equals(encryptionType, Aes256Sha256, StringComparison.OrdinalIgnoreCase)
            || string.Equals(encryptionType, TripleDesSha1, StringComparison.OrdinalIgnoreCase);
    }
}

