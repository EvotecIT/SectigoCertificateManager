namespace SectigoCertificateManager.Requests;

/// <summary>
/// Represents a public ACME account creation request.
/// </summary>
public sealed class AdminPublicAcmeAccountCreateRequest {
    public string AcmeServer { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string? CertValidationType { get; set; }
    public string? ValidationId { get; set; }
}

/// <summary>
/// Represents a universal (private) ACME account creation request.
/// </summary>
public sealed class AdminPrivateAcmeAccountCreateRequest {
    public string AcmeServer { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int OrganizationId { get; set; }
    public string ProfileName { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single domain name entry used when adding or removing domains from an ACME account.
/// </summary>
public sealed class AdminAcmeDomainNameRequest {
    public string Name { get; set; } = string.Empty;
    public string? ValidationId { get; set; }
}

