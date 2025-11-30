namespace SectigoCertificateManager.Requests;

/// <summary>
/// Represents a domain request payload used by DCV endpoints.
/// </summary>
public sealed class AdminDomainRequest {
    /// <summary>Gets or sets the domain name to validate.</summary>
    public string Domain { get; set; } = string.Empty;
}

/// <summary>
/// Represents an email-based DCV request.
/// </summary>
public sealed class DomainEmailRequest {
    /// <summary>Domain name to validate via email.</summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>Email address that will receive the DCV email.</summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Represents a bulk email DCV request.
/// </summary>
public sealed class DomainEmailBulkRequest {
    /// <summary>Domains to validate via email in a single request.</summary>
    public IReadOnlyList<string> Domains { get; set; } = Array.Empty<string>();

    /// <summary>Email address used for all domains in the bulk request.</summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Represents a CNAME-based DCV request.
/// </summary>
public sealed class DomainCnameRequest {
    /// <summary>Primary domain name to validate.</summary>
    public string Domain { get; set; } = string.Empty;

    /// <summary>Additional domains to validate in the same request.</summary>
    public IReadOnlyList<string>? Domains { get; set; }

    /// <summary>Identifier of the DNS agent to use for automation.</summary>
    public string? DnsAgentUuid { get; set; }

    /// <summary>Name of the DNS provider when using provider-assisted validation.</summary>
    public string? DnsProviderName { get; set; }
}
