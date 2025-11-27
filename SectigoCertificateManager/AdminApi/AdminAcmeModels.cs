namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a public ACME account entry returned by the Admin Operations API.
/// </summary>
public sealed class AdminPublicAcmeAccount {
    public int Id { get; set; }
    public string? AccountId { get; set; }
    public string? MacId { get; set; }
    public string? MacKey { get; set; }
    public string? AcmeServer { get; set; }
    public string? Name { get; set; }
    public int OrganizationId { get; set; }
    public string? CertValidationType { get; set; }
    public string? ValidationId { get; set; }
}

/// <summary>
/// Represents a public ACME domain entry associated with an account.
/// </summary>
public sealed class AdminPublicAcmeDomain {
    public string? Name { get; set; }
    public string? ValidUntil { get; set; }
    public string? StickyUntil { get; set; }
    public string? ValidationId { get; set; }
}

/// <summary>
/// Represents a public ACME client entry associated with an account.
/// </summary>
public sealed class AdminPublicAcmeClient {
    public int Id { get; set; }
    public string? AccountId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Status { get; set; }
    public string? LastActivity { get; set; }
    public string? Contacts { get; set; }
}

/// <summary>
/// Represents a universal (private) ACME account entry.
/// </summary>
public sealed class AdminPrivateAcmeAccount {
    public int Id { get; set; }
    public string? AccountId { get; set; }
    public string? MacId { get; set; }
    public string? MacKey { get; set; }
    public string? AcmeServer { get; set; }
    public string? Name { get; set; }
    public int OrganizationId { get; set; }
    public string? ProfileName { get; set; }
}

