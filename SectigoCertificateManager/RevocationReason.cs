namespace SectigoCertificateManager;

/// <summary>
/// Enumerates certificate revocation reasons.
/// </summary>

public enum RevocationReason {
    /// <summary>Unspecified reason.</summary>
    Unspecified = 0,

    /// <summary>Key compromise occurred.</summary>
    KeyCompromise = 1,

    /// <summary>CA compromise occurred.</summary>
    CaCompromise = 2,

    /// <summary>Affiliation changed.</summary>
    AffiliationChanged = 3,

    /// <summary>Certificate was superseded.</summary>
    Superseded = 4,

    /// <summary>Operations ceased.</summary>
    CessationOfOperation = 5,

    /// <summary>Certificate hold was applied.</summary>
    CertificateHold = 6,

    /// <summary>Certificate was removed from CRL.</summary>
    RemoveFromCrl = 8,

    /// <summary>Privilege was withdrawn.</summary>
    PrivilegeWithdrawn = 9,

    /// <summary>Attribute authority compromise occurred.</summary>
    AaCompromise = 10
}
