namespace SectigoCertificateManager;

/// <summary>
/// Enumerates certificate revocation reasons.
/// </summary>

public enum RevocationReason {
    /// <summary>Unspecified reason.</summary>
    Unspecified = 0,

    /// <summary>Key compromise occurred.</summary>
    KeyCompromise = 1,

    /// <summary>Affiliation changed.</summary>
    AffiliationChanged = 3,

    /// <summary>Certificate was superseded.</summary>
    Superseded = 4,

    /// <summary>Operations ceased.</summary>
    CessationOfOperation = 5
}