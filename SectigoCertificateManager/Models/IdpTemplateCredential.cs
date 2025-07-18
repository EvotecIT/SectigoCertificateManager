namespace SectigoCertificateManager.Models;

/// <summary>
/// Represents a credential used in an IdP template.
/// </summary>
public sealed class IdpTemplateCredential {
    /// <summary>Gets or sets the role.</summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrgId { get; set; }
}
