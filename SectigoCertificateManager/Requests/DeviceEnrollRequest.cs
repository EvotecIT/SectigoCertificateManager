namespace SectigoCertificateManager.Requests;

using System.Collections.Generic;

/// <summary>
/// Request payload used when enrolling a new device certificate via the Admin API.
/// </summary>
public sealed class DeviceEnrollRequest {
    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the certificate validity period in days.</summary>
    public int Term { get; set; }

    /// <summary>Gets or sets the certificate signing request.</summary>
    public string Csr { get; set; } = string.Empty;

    /// <summary>Gets or sets the certificate profile identifier.</summary>
    public int CertType { get; set; }

    /// <summary>
    /// Gets or sets custom fields to be applied to the requested certificate.
    /// </summary>
    public IReadOnlyList<AdminApiCertField>? CustomFields { get; set; }

    /// <summary>
    /// Gets or sets optional fields to be applied to the requested certificate.
    /// </summary>
    public IReadOnlyList<AdminApiCertField>? OptionalFields { get; set; }

    /// <summary>Gets or sets comments for the enroll request.</summary>
    public string? Comments { get; set; }
}

/// <summary>
/// Represents a name/value pair used when sending custom or optional fields to the Admin API.
/// </summary>
public sealed class AdminApiCertField {
    /// <summary>Gets or sets the field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the field value.</summary>
    public string Value { get; set; } = string.Empty;
}

