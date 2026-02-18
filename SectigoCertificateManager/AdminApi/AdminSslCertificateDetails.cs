namespace SectigoCertificateManager.AdminApi;

using SectigoCertificateManager.Utilities;
using System.Collections.Generic;
using System.Text.Json.Serialization;

/// <summary>
/// Represents detailed SSL certificate information returned by the Admin API.
/// </summary>
public sealed class AdminSslCertificateDetails {
    /// <summary>Gets or sets the certificate identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the certificate common name.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets the organization identifier.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the certificate status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the legacy order number for this certificate.</summary>
    public long OrderNumber { get; set; }

    /// <summary>Gets or sets the backend certificate identifier.</summary>
    public string? BackendCertId { get; set; }

    /// <summary>Gets or sets the issuing CA name.</summary>
    public string? Vendor { get; set; }

    /// <summary>Gets or sets the certificate term in days.</summary>
    public int Term { get; set; }

    /// <summary>Gets or sets the owner.</summary>
    public string? Owner { get; set; }

    /// <summary>Gets or sets the requester.</summary>
    public string? Requester { get; set; }

    /// <summary>Gets or sets the external requester (when the request originates outside the organization).</summary>
    public string? ExternalRequester { get; set; }

    /// <summary>Gets or sets additional comments.</summary>
    public string? Comments { get; set; }

    /// <summary>Gets or sets the requested date.</summary>
    public string? Requested { get; set; }

    /// <summary>Gets or sets the expiration date.</summary>
    public string? Expires { get; set; }

    /// <summary>Gets or sets the serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the key algorithm (deprecated in API).</summary>
    public string? KeyAlgorithm { get; set; }

    /// <summary>Gets or sets the key size (deprecated in API).</summary>
    public int? KeySize { get; set; }

    /// <summary>Gets or sets the key type description.</summary>
    public string? KeyType { get; set; }

    /// <summary>
    /// Gets or sets key usage flags returned by Admin API details (for example:
    /// <c>Digital Signature</c>, <c>Key Encipherment</c>).
    /// </summary>
    public IReadOnlyList<string>? KeyUsages { get; set; }

    /// <summary>
    /// Gets or sets extended key usages returned by Admin API details (friendly names or OIDs).
    /// </summary>
    public IReadOnlyList<string>? ExtendedKeyUsages { get; set; }

    /// <summary>
    /// Gets or sets usage purpose labels when returned by the Admin API details payload.
    /// </summary>
    public IReadOnlyList<string>? UsagePurposes { get; set; }

    /// <summary>Gets or sets subject alternative names.</summary>
    public IReadOnlyList<string>? SubjectAlternativeNames { get; set; }

    /// <summary>Gets or sets the revocation date.</summary>
    public string? Revoked { get; set; }

    /// <summary>Gets or sets the revocation reason code.</summary>
    [JsonConverter(typeof(JsonStringOrNumberConverter))]
    public string? ReasonCode { get; set; }

    /// <summary>Gets or sets a value indicating whether notifications are suspended.</summary>
    public bool SuspendNotifications { get; set; }
}
