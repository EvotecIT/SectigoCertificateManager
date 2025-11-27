namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a domain entry returned by the Admin API.
/// </summary>
public sealed class AdminDomainInfo {
    /// <summary>Gets or sets the domain identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the domain name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the domain delegation status.</summary>
    public string? DelegationStatus { get; set; }

    /// <summary>Gets or sets the domain state.</summary>
    public string? State { get; set; }

    /// <summary>Gets or sets the domain validation status.</summary>
    public string? ValidationStatus { get; set; }

    /// <summary>Gets or sets the domain validation method.</summary>
    public string? ValidationMethod { get; set; }

    /// <summary>Gets or sets the DCV validation date (yyyy-MM-dd).</summary>
    public string? DcvValidation { get; set; }

    /// <summary>Gets or sets the DCV expiration date (yyyy-MM-dd).</summary>
    public string? DcvExpiration { get; set; }

    /// <summary>Gets or sets CT log monitoring settings.</summary>
    public AdminCtLogMonitoring? CtLogMonitoring { get; set; }

    /// <summary>Gets or sets domain delegations.</summary>
    public IReadOnlyList<AdminDomainDelegation>? Delegations { get; set; }
}

/// <summary>
/// Represents CT log monitoring settings for a domain.
/// </summary>
public sealed class AdminCtLogMonitoring {
    /// <summary>Gets or sets a value indicating whether monitoring is enabled.</summary>
    public bool Enabled { get; set; }
}

/// <summary>
/// Represents a single domain delegation entry.
/// </summary>
public sealed class AdminDomainDelegation {
    /// <summary>Gets or sets the organization or department identifier.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the certificate types associated with the delegation.</summary>
    public IReadOnlyList<string> CertTypes { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets domain certificate request privileges.</summary>
    public IReadOnlyList<string> DomainCertificateRequestPrivileges { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets the delegation status.</summary>
    public string? Status { get; set; }
}

