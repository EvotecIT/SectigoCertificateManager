using SectigoCertificateManager.Models;
using System.Collections.Generic;

namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload used when updating a certificate.
/// </summary>
public sealed class UpdateCertificateRequest
{
    /// <summary>Gets or sets the certificate identifier.</summary>
    public int SslId { get; set; }

    /// <summary>Gets or sets the certificate term.</summary>
    public int? Term { get; set; }

    /// <summary>Gets or sets the certificate profile identifier.</summary>
    public int? CertTypeId { get; set; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public int? OrgId { get; set; }

    /// <summary>Gets or sets the certificate common name.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets the CSR.</summary>
    public string? Csr { get; set; }

    /// <summary>Gets or sets external requester emails.</summary>
    public string? ExternalRequester { get; set; }

    /// <summary>Gets or sets comments.</summary>
    public string? Comments { get; set; }

    /// <summary>Gets or sets subject alternative names.</summary>
    public IReadOnlyList<string> SubjectAlternativeNames { get; set; } = [];

    /// <summary>Gets or sets custom fields.</summary>
    public IReadOnlyList<CustomField> CustomFields { get; set; } = [];

    /// <summary>Gets or sets auto renew details.</summary>
    public AutoRenewDetails? AutoRenewDetails { get; set; }

    /// <summary>Gets or sets whether notifications should be suspended.</summary>
    public bool? SuspendNotifications { get; set; }

    /// <summary>Gets or sets requester.</summary>
    public string? Requester { get; set; }

    /// <summary>Gets or sets requester admin identifier.</summary>
    public int? RequesterAdminId { get; set; }

    /// <summary>Gets or sets approver admin identifier.</summary>
    public int? ApproverAdminId { get; set; }
}
