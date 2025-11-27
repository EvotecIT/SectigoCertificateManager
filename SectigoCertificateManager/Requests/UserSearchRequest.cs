namespace SectigoCertificateManager.Requests;

/// <summary>
/// Filter parameters for searching users.
/// </summary>
public sealed class UserSearchRequest {
    /// <summary>Zero-based position offset for paging.</summary>
    public int? Position { get; set; }
    /// <summary>Page size for paging.</summary>
    public int? Size { get; set; }
    /// <summary>Full or partial name filter.</summary>
    public string? Name { get; set; }
    /// <summary>Organization identifier filter.</summary>
    public int? OrganizationId { get; set; }
    /// <summary>Email address filter.</summary>
    public string? Email { get; set; }
    /// <summary>Common name filter.</summary>
    public string? CommonName { get; set; }
    /// <summary>Secondary email filter.</summary>
    public string? SecondaryEmail { get; set; }
    /// <summary>Phone number filter.</summary>
    public string? Phone { get; set; }
}
