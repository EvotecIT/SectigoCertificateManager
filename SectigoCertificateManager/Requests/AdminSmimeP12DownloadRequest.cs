namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload used when downloading a S/MIME certificate in P12 format.
/// </summary>
public sealed class AdminSmimeP12DownloadRequest {
    /// <summary>Gets or sets the P12 passphrase.</summary>
    public string? Passphrase { get; set; }

    /// <summary>
    /// Gets or sets the P12 encryption type (for example, "AES256-SHA256" or "TripleDES-SHA1").
    /// </summary>
    public string? EncryptionType { get; set; }
}

