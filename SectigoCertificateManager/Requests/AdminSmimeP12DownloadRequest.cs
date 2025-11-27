namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload used when downloading a S/MIME certificate in P12 format.
/// </summary>
public sealed class AdminSmimeP12DownloadRequest {
    /// <summary>Gets or sets the P12 passphrase.</summary>
    public string? Passphrase { get; set; }

    /// <summary>
    /// Gets or sets the P12 encryption type.
    /// <para>
    /// Supported values are defined by the Admin Operations API (for example,
    /// <see cref="AdminSmimeP12EncryptionTypes.Aes256Sha256"/> or
    /// <see cref="AdminSmimeP12EncryptionTypes.TripleDesSha1"/>).
    /// </para>
    /// </summary>
    public string? EncryptionType { get; set; }
}
