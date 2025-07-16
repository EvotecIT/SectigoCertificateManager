namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload for updating a user.
/// </summary>
public sealed class UpdateUserRequest {
    public string? FirstName { get; set; }
    public string? MiddleName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? ValidationType { get; set; }
    public int? OrganizationId { get; set; }
    public string? Phone { get; set; }
    public string? CommonName { get; set; }
    public IReadOnlyList<string>? SecondaryEmails { get; set; }
    public string? Eppn { get; set; }
    public string? Upn { get; set; }
}
