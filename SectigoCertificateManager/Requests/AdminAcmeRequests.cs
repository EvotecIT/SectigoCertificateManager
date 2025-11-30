namespace SectigoCertificateManager.Requests;

/// <summary>
/// Represents a public ACME account creation request.
/// </summary>
public sealed class AdminPublicAcmeAccountCreateRequest {
    /// <summary>ACME server URL (for example, https://acme.sectigo.com).</summary>
    public string AcmeServer { get; set; } = string.Empty;
    /// <summary>Display name for the ACME account.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Organization identifier that owns the account.</summary>
    public int OrganizationId { get; set; }
    /// <summary>Certificate validation type string required by the API.</summary>
    public string? CertValidationType { get; set; }
    /// <summary>Validation identifier returned by the ACME service, when applicable.</summary>
    public string? ValidationId { get; set; }
}

/// <summary>
/// Represents a universal (private) ACME account creation request.
/// </summary>
public sealed class AdminPrivateAcmeAccountCreateRequest {
    /// <summary>ACME server URL (for example, https://acme.sectigo.com).</summary>
    public string AcmeServer { get; set; } = string.Empty;
    /// <summary>Display name for the ACME account.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Organization identifier that owns the account.</summary>
    public int OrganizationId { get; set; }
    /// <summary>Certificate profile name to associate with the account.</summary>
    public string ProfileName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single domain name entry used when adding or removing domains from an ACME account.
/// </summary>
public sealed class AdminAcmeDomainNameRequest {
    /// <summary>Domain name to add or remove.</summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>Optional validation identifier associated with the domain.</summary>
    public string? ValidationId { get; set; }
}
