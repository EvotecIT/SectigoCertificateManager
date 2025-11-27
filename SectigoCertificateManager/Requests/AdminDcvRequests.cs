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
    public string Domain { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Represents a bulk email DCV request.
/// </summary>
public sealed class DomainEmailBulkRequest {
    public IReadOnlyList<string> Domains { get; set; } = Array.Empty<string>();

    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Represents a CNAME-based DCV request.
/// </summary>
public sealed class DomainCnameRequest {
    public string Domain { get; set; } = string.Empty;

    public IReadOnlyList<string>? Domains { get; set; }

    public string? DnsAgentUuid { get; set; }

    public string? DnsProviderName { get; set; }
}
