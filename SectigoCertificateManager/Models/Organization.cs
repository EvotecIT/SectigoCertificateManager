namespace SectigoCertificateManager.Models;

using System.Collections.Generic;

/// <summary>
/// Represents an organization returned by the Sectigo API.
/// </summary>
public sealed class Organization {
    /// <summary>Gets or sets the organization identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the organization name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the organization alias.</summary>
    public string? Alias { get; set; }

    /// <summary>Gets or sets the organization secondary name.</summary>
    public string? SecondaryName { get; set; }

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
    public string? StateOrProvince { get; set; }

    /// <summary>Gets or sets the postal code.</summary>
    public string? PostalCode { get; set; }

    /// <summary>Gets or sets the country code.</summary>
    public string? Country { get; set; }

    /// <summary>Gets or sets a value indicating whether SSL certificates API is enabled.</summary>
    public bool SslCertsApiEnabled { get; set; }

    /// <summary>Gets or sets a value indicating whether client certificates API is enabled.</summary>
    public bool ClientCertsApiEnabled { get; set; }

    /// <summary>Gets or sets allowed certificate types.</summary>
    public IReadOnlyList<string> CertTypes { get; set; } = [];

    /// <summary>Gets or sets departments belonging to the organization.</summary>
    public IReadOnlyList<OrganizationDepartment> Departments { get; set; } = [];

    /// <summary>Gets or sets client certificate options.</summary>
    public OrganizationClientCertificate? ClientCertificate { get; set; }
}

/// <summary>
/// Represents department information returned with an organization.
/// </summary>
public sealed class OrganizationDepartment {
    /// <summary>Gets or sets the department identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the department name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent organization name.</summary>
    public string? ParentName { get; set; }
}

/// <summary>
/// Represents client certificate settings for an organization.
/// </summary>
public sealed class OrganizationClientCertificate {
    /// <summary>Gets or sets a value indicating whether master admins can recover keys.</summary>
    public bool AllowKeyRecoveryByMasterAdmins { get; set; }

    /// <summary>Gets or sets a value indicating whether organization admins can recover keys.</summary>
    public bool AllowKeyRecoveryByOrgAdmins { get; set; }

    /// <summary>Gets or sets a value indicating whether department admins can recover keys.</summary>
    public bool AllowKeyRecoveryByDepartmentAdmins { get; set; }
}