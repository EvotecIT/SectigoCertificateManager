namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a basic organization entry returned by the Admin Operations API.
/// </summary>
public sealed class AdminOrganizationBasic {
    /// <summary>Gets or sets the organization identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the organization name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent organization name, when available.</summary>
    public string? ParentName { get; set; }

    /// <summary>Gets or sets basic department information for this organization.</summary>
    public IReadOnlyList<AdminDepartmentBasicInfo> Departments { get; set; } = Array.Empty<AdminDepartmentBasicInfo>();
}

/// <summary>
/// Represents a basic department entry associated with an organization.
/// </summary>
public sealed class AdminDepartmentBasicInfo {
    /// <summary>Gets or sets the department identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the parent organization name.</summary>
    public string? ParentName { get; set; }

    /// <summary>Gets or sets the department name.</summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Represents detailed organization information returned by the Admin Operations API.
/// </summary>
public sealed class AdminOrganizationDetails {
    /// <summary>Gets or sets the organization identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the organization name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the parent organization identifier.</summary>
    public int? ParentId { get; set; }

    /// <summary>Gets or sets the parent organization name.</summary>
    public string? ParentName { get; set; }

    /// <summary>Gets or sets the alternative organization name.</summary>
    public string? SecondaryName { get; set; }

    /// <summary>Gets or sets allowed certificate types (for example, SSL, SMIME).</summary>
    public IReadOnlyList<string> CertTypes { get; set; } = Array.Empty<string>();

    /// <summary>Gets or sets the organization address line 1.</summary>
    public string? Address1 { get; set; }

    /// <summary>Gets or sets the organization address line 2.</summary>
    public string? Address2 { get; set; }

    /// <summary>Gets or sets the organization address line 3.</summary>
    public string? Address3 { get; set; }

    /// <summary>Gets or sets the city.</summary>
    public string? City { get; set; }

    /// <summary>Gets or sets the state or province.</summary>
    public string? StateOrProvince { get; set; }

    /// <summary>Gets or sets the postal code.</summary>
    public string? PostalCode { get; set; }

    /// <summary>Gets or sets the country code (ISO 3166-1 alpha-2).</summary>
    public string? Country { get; set; }

    /// <summary>Gets or sets organization departments.</summary>
    public IReadOnlyList<AdminDepartmentBasicInfo> Departments { get; set; } = Array.Empty<AdminDepartmentBasicInfo>();
}

