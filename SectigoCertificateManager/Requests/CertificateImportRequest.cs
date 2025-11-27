namespace SectigoCertificateManager.Requests;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents a certificate import request used by the Admin Operations API.
/// </summary>
public sealed class CertificateImportRequest {
    /// <summary>
    /// Gets or sets the organization identifier.
    /// </summary>
    [JsonPropertyName("orgID")]
    public int OrgId { get; set; }

    /// <summary>
    /// Gets or sets custom fields applied to the imported certificate.
    /// </summary>
    public IReadOnlyList<AdminApiCertField>? CustomFields { get; set; }

    /// <summary>
    /// Gets or sets the certificate owner.
    /// </summary>
    public string? Owner { get; set; }

    /// <summary>
    /// Gets or sets the external requester identifier.
    /// </summary>
    public string? ExternalRequester { get; set; }

    /// <summary>
    /// Gets or sets the backend certificate identifier used by the CA.
    /// </summary>
    public string? BackendCertId { get; set; }

    /// <summary>
    /// Gets or sets the certificate contents in base64 form.
    /// </summary>
    public string Cert { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the certificate signing request associated with the import, when available.
    /// </summary>
    public string? Csr { get; set; }

    /// <summary>
    /// Gets or sets comments associated with the import operation.
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Gets or sets the licensed SANs count.
    /// </summary>
    public int? LicensedSansCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the license service should be updated.
    /// </summary>
    public bool? UpdateLicenseService { get; set; }

    /// <summary>
    /// Gets or sets the revocation date for the imported certificate, when applicable.
    /// </summary>
    public DateTimeOffset? RevokeDate { get; set; }

    /// <summary>
    /// Gets or sets the renewal date for the imported certificate, when applicable.
    /// </summary>
    public DateTimeOffset? RenewDate { get; set; }

    /// <summary>
    /// Gets or sets the license key associated with the imported certificate.
    /// </summary>
    public string? LicenseKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the import should be forced when conflicts occur.
    /// </summary>
    public bool? Force { get; set; }

    /// <summary>
    /// Gets or sets the revocation reason code associated with the import, when applicable.
    /// </summary>
    public string? ReasonCode { get; set; }
}

