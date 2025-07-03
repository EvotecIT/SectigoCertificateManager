namespace SectigoCertificateManager;

/// <summary>
/// Enumerates statuses for certificates.
/// </summary>
public enum CertificateStatus
{
    /// <summary>Any status.</summary>
    Any = 0,

    /// <summary>Request for certificate has been submitted.</summary>
    Requested = 1,

    /// <summary>The certificate has been approved.</summary>
    Approved = 8,

    /// <summary>The certificate request was applied.</summary>
    Applied = 9,

    /// <summary>The certificate has been issued.</summary>
    Issued = 2,

    /// <summary>The certificate has been revoked.</summary>
    Revoked = 3,

    /// <summary>The certificate has expired.</summary>
    Expired = 4
}
