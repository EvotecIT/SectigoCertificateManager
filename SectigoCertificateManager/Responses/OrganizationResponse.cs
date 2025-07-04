namespace SectigoCertificateManager.Responses;

using SectigoCertificateManager.Models;

/// <summary>
/// Represents a response containing organization information.
/// </summary>
public sealed class OrganizationResponse
{
    /// <summary>
    /// Gets or sets the organization.
    /// </summary>
    public Organization? Organization { get; set; }
}
