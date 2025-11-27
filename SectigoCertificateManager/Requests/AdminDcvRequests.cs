namespace SectigoCertificateManager.Requests;

/// <summary>
/// Represents a domain request payload used by DCV endpoints.
/// </summary>
public sealed class AdminDomainRequest {
    /// <summary>Gets or sets the domain name to validate.</summary>
    public string Domain { get; set; } = string.Empty;
}

