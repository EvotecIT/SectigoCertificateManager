namespace SectigoCertificateManager.Requests;
using SectigoCertificateManager.Models;

/// <summary>
/// Request payload used to create an organization.
/// </summary>
public sealed class CreateOrganizationRequest {
    /// <summary>Gets or sets the organization name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the alternative name.</summary>
    public string? AlternativeName { get; set; }

    /// <summary>Gets or sets the SCHAC home organization (deprecated).</summary>
    public string? SchacHomeOrganization { get; set; }

    /// <summary>Gets or sets the organization alias.</summary>
    public string? Alias { get; set; }

    /// <summary>Gets or sets contact emails separated by comma.</summary>
    public string? ContactEmails { get; set; }

    /// <summary>Gets or sets the contact webhook URL.</summary>
    public string? ContactWebhook { get; set; }

    /// <summary>Gets or sets the contact Slack webhook URL.</summary>
    public string? ContactSlack { get; set; }

    /// <summary>Gets or sets the contact Teams webhook URL.</summary>
    public string? ContactTeams { get; set; }

    /// <summary>Gets or sets the first address line.</summary>
    public string? Address1 { get; set; }

    /// <summary>Gets or sets the second address line.</summary>
    public string? Address2 { get; set; }

    /// <summary>Gets or sets the third address line.</summary>
    public string? Address3 { get; set; }

    /// <summary>Gets or sets the city.</summary>
    public string? City { get; set; }

    /// <summary>Gets or sets the state or province.</summary>
    public string? StateProvince { get; set; }

    /// <summary>Gets or sets the postal code.</summary>
    public string? PostalCode { get; set; }

    /// <summary>Gets or sets the country code.</summary>
    public string? Country { get; set; }

    /// <summary>Gets or sets client certificate options.</summary>
    public OrganizationClientCertificate? ClientCertificate { get; set; }

    /// <summary>Gets or sets a value indicating whether SSL certificates API is enabled.</summary>
    public bool SslCertsApiEnabled { get; set; }

    /// <summary>Gets or sets a value indicating whether client certificates API is enabled.</summary>
    public bool ClientCertsApiEnabled { get; set; }
}