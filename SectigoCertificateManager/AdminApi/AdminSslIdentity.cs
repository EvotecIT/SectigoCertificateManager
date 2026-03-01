namespace SectigoCertificateManager.AdminApi;

/// <summary>
/// Represents a certificate entry returned by the Admin API SSL list endpoint.
/// </summary>
public sealed class AdminSslIdentity {
    /// <summary>Gets or sets the SSL identifier (sslId).</summary>
    public int SslId { get; set; }

    /// <summary>Gets or sets the common name (typically present for list results).</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets the certificate serial number (optional in list projections).</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the subject alternative names (optional in list projections).</summary>
    public IReadOnlyList<string>? SubjectAlternativeNames { get; set; }

    /// <summary>Gets or sets external requester information, when provided by the list endpoint.</summary>
    public string? ExternalRequester { get; set; }

    /// <summary>Gets or sets the legacy order number (0 when omitted by the list endpoint).</summary>
    public long OrderNumber { get; set; }

    /// <summary>Gets or sets the organization identifier (0 when omitted by the list endpoint).</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the certificate status text (optional in list projections).</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the requester (optional in list projections).</summary>
    public string? Requester { get; set; }

    /// <summary>Gets or sets the owner (optional in list projections).</summary>
    public string? Owner { get; set; }

    /// <summary>Gets or sets the vendor (optional in list projections).</summary>
    public string? Vendor { get; set; }

    /// <summary>Gets or sets the term in days (0 when omitted by the list endpoint).</summary>
    public int Term { get; set; }

    /// <summary>Gets or sets requested timestamp text (optional in list projections).</summary>
    public string? Requested { get; set; }

    /// <summary>Gets or sets expiry timestamp text (optional in list projections).</summary>
    public string? Expires { get; set; }

    /// <summary>Gets or sets suspend-notification state when available; null when not provided.</summary>
    public bool? SuspendNotifications { get; set; }
}
