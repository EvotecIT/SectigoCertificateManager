namespace SectigoCertificateManager.Requests;

using SectigoCertificateManager.AdminApi;
using System.Collections.Generic;

/// <summary>
/// Request payload used to create a domain via the Admin API.
/// </summary>
public sealed class CreateDomainRequest {
    /// <summary>Gets or sets the domain name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the domain description.</summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the active state as a string ("true" or "false") as defined by the Admin API.
    /// </summary>
    public string Active { get; set; } = "true";

    /// <summary>Gets or sets CT log monitoring settings.</summary>
    public AdminCtLogMonitoring? CtLogMonitoring { get; set; }

    /// <summary>
    /// Gets or sets domain delegations to organizations or departments.
    /// At least one delegation is required by the Admin API.
    /// </summary>
    public IReadOnlyList<AdminDomainDelegation> Delegations { get; set; } = new List<AdminDomainDelegation>();
}

