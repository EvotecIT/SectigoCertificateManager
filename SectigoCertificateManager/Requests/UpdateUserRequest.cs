namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload for updating a user.
/// </summary>
public sealed class UpdateUserRequest {
    /// <summary>Given name of the user.</summary>
    public string? FirstName { get; set; }
    /// <summary>Middle name of the user.</summary>
    public string? MiddleName { get; set; }
    /// <summary>Surname of the user.</summary>
    public string? LastName { get; set; }
    /// <summary>Primary email address.</summary>
    public string? Email { get; set; }
    /// <summary>Validation type string as required by the API.</summary>
    public string? ValidationType { get; set; }
    /// <summary>Organization identifier the user belongs to.</summary>
    public int? OrganizationId { get; set; }
    /// <summary>Contact phone number.</summary>
    public string? Phone { get; set; }
    /// <summary>Common name to use for certificates.</summary>
    public string? CommonName { get; set; }
    /// <summary>Optional secondary email addresses.</summary>
    public IReadOnlyList<string>? SecondaryEmails { get; set; }
    /// <summary>EduPersonPrincipalName for SSO scenarios.</summary>
    public string? Eppn { get; set; }
    /// <summary>User Principal Name for SSO scenarios.</summary>
    public string? Upn { get; set; }
}
