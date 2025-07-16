namespace SectigoCertificateManager.Requests;

/// <summary>
/// Filter parameters for searching users.
/// </summary>
public sealed class UserSearchRequest {
    public int? Position { get; set; }
    public int? Size { get; set; }
    public string? Name { get; set; }
    public int? OrganizationId { get; set; }
    public string? Email { get; set; }
    public string? CommonName { get; set; }
    public string? SecondaryEmail { get; set; }
    public string? Phone { get; set; }
}
