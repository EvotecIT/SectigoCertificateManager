namespace SectigoCertificateManager.Models;

/// <summary>
/// Represents a user (person) entry.
/// </summary>
public sealed class User {
    /// <summary>Gets or sets the user identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrganizationId { get; set; }

    /// <summary>Gets or sets the user email address.</summary>
    public string? Email { get; set; }

    /// <summary>Gets or sets the first name.</summary>
    public string? FirstName { get; set; }

    /// <summary>Gets or sets the last name.</summary>
    public string? LastName { get; set; }

    /// <summary>Gets or sets the middle name.</summary>
    public string? MiddleName { get; set; }

    /// <summary>Gets or sets the validation type.</summary>
    public string? ValidationType { get; set; }

    /// <summary>Gets or sets the phone number.</summary>
    public string? Phone { get; set; }

    /// <summary>Gets or sets the common name.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets secondary email addresses.</summary>
    public IReadOnlyList<string>? SecondaryEmails { get; set; }

    /// <summary>Gets or sets the EPPN identifier.</summary>
    public string? Eppn { get; set; }

    /// <summary>Gets or sets the UPN identifier.</summary>
    public string? Upn { get; set; }

    /// <summary>Gets or sets the creation timestamp.</summary>
    public DateTimeOffset? Created { get; set; }

    /// <summary>Gets or sets the creator.</summary>
    public string? CreatedBy { get; set; }

    /// <summary>Gets or sets the last modification timestamp.</summary>
    public DateTimeOffset? Modified { get; set; }

    /// <summary>Gets or sets the modifier.</summary>
    public string? ModifiedBy { get; set; }
}
