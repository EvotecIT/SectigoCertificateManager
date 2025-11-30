namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a public ACME account entry returned by the Admin Operations API.
/// </summary>
public sealed class AdminPublicAcmeAccount {
    /// <summary>Gets or sets the internal Sectigo account identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the ACME account identifier assigned by the ACME server.</summary>
    public string? AccountId { get; set; }

    /// <summary>Gets or sets the MAC key identifier used for HMAC‑based authentication.</summary>
    public string? MacId { get; set; }

    /// <summary>Gets or sets the MAC key value used for HMAC‑based authentication.</summary>
    public string? MacKey { get; set; }

    /// <summary>Gets or sets the ACME server URL associated with this account.</summary>
    public string? AcmeServer { get; set; }

    /// <summary>Gets or sets the friendly name of the ACME account.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the organization identifier associated with this account.</summary>
    public int OrganizationId { get; set; }

    /// <summary>Gets or sets the certificate validation type (for example, DV, OV, EV).</summary>
    public string? CertValidationType { get; set; }

    /// <summary>Gets or sets the validation identifier used to track account validation.</summary>
    public string? ValidationId { get; set; }
}

/// <summary>
/// Represents a public ACME domain entry associated with an account.
/// </summary>
public sealed class AdminPublicAcmeDomain {
    /// <summary>Gets or sets the fully qualified domain name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the date until which the domain is valid.</summary>
    public string? ValidUntil { get; set; }

    /// <summary>Gets or sets the date until which the domain mapping is sticky.</summary>
    public string? StickyUntil { get; set; }

    /// <summary>Gets or sets the validation identifier for the domain.</summary>
    public string? ValidationId { get; set; }
}

/// <summary>
/// Represents a public ACME client entry associated with an account.
/// </summary>
public sealed class AdminPublicAcmeClient {
    /// <summary>Gets or sets the internal client identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the ACME account identifier associated with this client.</summary>
    public string? AccountId { get; set; }

    /// <summary>Gets or sets the last known client IP address.</summary>
    public string? IpAddress { get; set; }

    /// <summary>Gets or sets the user agent string reported by the client.</summary>
    public string? UserAgent { get; set; }

    /// <summary>Gets or sets the client status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the timestamp of the last client activity.</summary>
    public string? LastActivity { get; set; }

    /// <summary>Gets or sets the ACME contact information associated with the client.</summary>
    public string? Contacts { get; set; }
}

/// <summary>
/// Represents a universal (private) ACME account entry.
/// </summary>
public sealed class AdminPrivateAcmeAccount {
    /// <summary>Gets or sets the internal account identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the ACME account identifier assigned by the private ACME server.</summary>
    public string? AccountId { get; set; }

    /// <summary>Gets or sets the MAC key identifier used for HMAC‑based authentication.</summary>
    public string? MacId { get; set; }

    /// <summary>Gets or sets the MAC key value used for HMAC‑based authentication.</summary>
    public string? MacKey { get; set; }

    /// <summary>Gets or sets the ACME server URL associated with this account.</summary>
    public string? AcmeServer { get; set; }

    /// <summary>Gets or sets the friendly name of the ACME account.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the organization identifier associated with this account.</summary>
    public int OrganizationId { get; set; }

    /// <summary>Gets or sets the profile name used for issuance through this account.</summary>
    public string? ProfileName { get; set; }
}

/// <summary>
/// Represents metadata about a public ACME server.
/// </summary>
public sealed class AcmeServerInfo {
    /// <summary>Gets or sets the ACME server URL.</summary>
    public string? Url { get; set; }

    /// <summary>Gets or sets the CA identifier used by this ACME server.</summary>
    public int? CaId { get; set; }

    /// <summary>Gets or sets the friendly ACME server name.</summary>
    public string? Name { get; set; }

    /// <summary>Gets or sets the product identifier for single‑domain certificates.</summary>
    public int? SingleProductId { get; set; }

    /// <summary>Gets or sets the product identifier for multi‑domain certificates.</summary>
    public int? MultiProductId { get; set; }

    /// <summary>Gets or sets the product identifier for wildcard certificates.</summary>
    public int? WcProductId { get; set; }

    /// <summary>Gets or sets the certificate validation type (for example, DV, OV, EV).</summary>
    public string? CertValidationType { get; set; }
}
