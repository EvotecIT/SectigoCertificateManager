namespace SectigoCertificateManager.Models;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a S/MIME (client) certificate summary returned by the Admin API.
/// </summary>
public sealed class AdminSmimeCertificate {
    /// <summary>Gets or sets the certificate identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the certificate state.</summary>
    public string? State { get; set; }

    /// <summary>Gets or sets the certificate serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the backend certificate identifier.</summary>
    public string? BackendCertId { get; set; }

    /// <summary>Gets or sets the expiration date.</summary>
    public DateTimeOffset? Expires { get; set; }

    /// <summary>Gets or sets comments associated with the certificate.</summary>
    public string? Comments { get; set; }
}

/// <summary>
/// Represents detailed S/MIME certificate information returned by the Admin API.
/// </summary>
public sealed class AdminSmimeCertificateDetails {
    /// <summary>Gets or sets the certificate identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the certificate status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the backend certificate identifier.</summary>
    public string? BackendCertId { get; set; }

    /// <summary>Gets or sets the term in days.</summary>
    public int Term { get; set; }

    /// <summary>Gets or sets the serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the expiration date.</summary>
    public DateTimeOffset? Expires { get; set; }

    /// <summary>Gets or sets certificate comments.</summary>
    public string? Comments { get; set; }
}

/// <summary>
/// Represents a S/MIME enrollment response.
/// </summary>
public sealed class AdminSmimeEnrollResponse {
    /// <summary>Gets or sets the backend certificate identifier.</summary>
    public string? BackendCertId { get; set; }
}

