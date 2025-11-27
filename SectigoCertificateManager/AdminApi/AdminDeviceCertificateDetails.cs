namespace SectigoCertificateManager.AdminApi;

/// <summary>
/// Represents detailed device certificate information returned by the Admin API.
/// </summary>
public sealed class AdminDeviceCertificateDetails {
    /// <summary>Gets or sets the issuing CA certificate subject.</summary>
    public string? Issuer { get; set; }

    /// <summary>Gets or sets the device certificate subject.</summary>
    public string? Subject { get; set; }

    /// <summary>Gets or sets the subject alternative names.</summary>
    public string? SubjectAltNames { get; set; }

    /// <summary>Gets or sets the MD5 fingerprint.</summary>
    public string? Md5Hash { get; set; }

    /// <summary>Gets or sets the SHA1 fingerprint.</summary>
    public string? Sha1Hash { get; set; }

    /// <summary>Gets or sets the SHA256 fingerprint.</summary>
    public string? Sha256Hash { get; set; }
}

