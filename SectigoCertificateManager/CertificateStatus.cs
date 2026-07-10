namespace SectigoCertificateManager;

using System.Text.Json.Serialization;

/// <summary>
/// Enumerates statuses for certificates.
/// </summary>
[JsonConverter(typeof(CertificateStatusJsonConverter))]
public enum CertificateStatus {
    /// <summary>Any status.</summary>
    Any = 0,

    /// <summary>Request for certificate has been submitted.</summary>
    Requested = 1,

    /// <summary>The certificate has been approved.</summary>
    Approved = 3,

    /// <summary>The certificate request was applied.</summary>
    Applied = 4,

    /// <summary>The certificate has been issued.</summary>
    Issued = 2,

    /// <summary>The certificate request was declined.</summary>
    Declined = 5,

    /// <summary>The certificate was downloaded. Deprecated.</summary>
    Downloaded = 6,

    /// <summary>The certificate request was rejected.</summary>
    Rejected = 7,

    /// <summary>The certificate awaits approval. Deprecated.</summary>
    AwaitingApproval = 8,

    /// <summary>The certificate status is invalid.</summary>
    Invalid = 9,

    /// <summary>The certificate was replaced.</summary>
    Replaced = 10,

    /// <summary>Certificate is unmanaged. Deprecated.</summary>
    Unmanaged = 11,

    /// <summary>The certificate was approved by system administrator.</summary>
    SAApproved = 12,

    /// <summary>The certificate request was initialized.</summary>
    Init = 13,

    /// <summary>The certificate has been revoked.</summary>
    Revoked = 14,

    /// <summary>The certificate has expired.</summary>
    Expired = 15,

    /// <summary>The certificate is enrolled and waiting to be downloaded.</summary>
    EnrolledPendingDownload = 16,

    /// <summary>The certificate subject has not been enrolled.</summary>
    NotEnrolled = 17,

    /// <summary>The certificate was registered as external. Deprecated.</summary>
    External = 18
}
