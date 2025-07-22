namespace SectigoCertificateManager;

/// <summary>
/// Enumerates statuses for certificates.
/// </summary>
public enum CertificateStatus {
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

    /// <summary>The certificate request was declined.</summary>
    Declined,

    /// <summary>The certificate was downloaded. Deprecated.</summary>
    Downloaded,

    /// <summary>The certificate request was rejected.</summary>
    Rejected,

    /// <summary>The certificate awaits approval. Deprecated.</summary>
    AwaitingApproval,

    /// <summary>The certificate status is invalid.</summary>
    Invalid,

    /// <summary>The certificate was replaced.</summary>
    Replaced,

    /// <summary>Certificate is unmanaged. Deprecated.</summary>
    Unmanaged,

    /// <summary>The certificate was approved by system administrator.</summary>
    SAApproved,

    /// <summary>The certificate request was initialized.</summary>
    Init,

    /// <summary>The certificate has been revoked.</summary>
    Revoked = 3,

    /// <summary>The certificate has expired.</summary>
    Expired = 4
}