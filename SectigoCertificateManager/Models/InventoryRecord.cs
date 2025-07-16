namespace SectigoCertificateManager.Models;

/// <summary>
/// Represents a record in an inventory CSV file.
/// </summary>
public sealed class InventoryRecord {
    /// <summary>Gets or sets the certificate identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the certificate common name.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets the organization name.</summary>
    public string? OrganizationName { get; set; }

    /// <summary>Gets or sets the certificate status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the expiration date.</summary>
    public string? Expires { get; set; }
}
