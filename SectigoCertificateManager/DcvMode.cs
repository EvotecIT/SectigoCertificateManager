namespace SectigoCertificateManager;

/// <summary>
/// Enumerates domain control validation modes supported by the Sectigo APIs.
/// </summary>
public enum DcvMode {
    /// <summary>No specific DCV mode.</summary>
    None = 0,

    /// <summary>Email-based DCV.</summary>
    Email,

    /// <summary>DCV via CNAME DNS record.</summary>
    Cname,

    /// <summary>DCV via HTTP file.</summary>
    Http,

    /// <summary>DCV via HTTPS file.</summary>
    Https,

    /// <summary>DCV via TXT DNS record.</summary>
    Txt
}

