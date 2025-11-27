namespace SectigoCertificateManager.Requests;

using System.Collections.Generic;

/// <summary>
/// Request payload used when enrolling a new S/MIME (client) certificate via the Admin API.
/// </summary>
public sealed class AdminSmimeEnrollRequest {
    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the person's first name.</summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>Gets or sets the person's middle name.</summary>
    public string? MiddleName { get; set; }

    /// <summary>Gets or sets the person's last name.</summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>Gets or sets the person's email address.</summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>Gets or sets the person's phone number.</summary>
    public string? Phone { get; set; }

    /// <summary>Gets or sets secondary email addresses.</summary>
    public IReadOnlyList<string>? SecondaryEmails { get; set; }

    /// <summary>Gets or sets the CSR.</summary>
    public string Csr { get; set; } = string.Empty;

    /// <summary>Gets or sets the certificate profile identifier.</summary>
    public int CertType { get; set; }

    /// <summary>Gets or sets the term in days.</summary>
    public int Term { get; set; }

    /// <summary>
    /// Gets or sets custom fields to be applied to the requested certificate.
    /// </summary>
    public IReadOnlyList<AdminApiCertField>? CustomFields { get; set; }

    /// <summary>Gets or sets the common name.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets comments for the enrollment request.</summary>
    public string? Comments { get; set; }
}
