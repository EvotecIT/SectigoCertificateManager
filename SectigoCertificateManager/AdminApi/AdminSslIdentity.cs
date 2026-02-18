namespace SectigoCertificateManager.AdminApi;

/// <summary>
/// Represents a certificate entry returned by the Admin API SSL list endpoint.
/// </summary>
public sealed class AdminSslIdentity {
    /// <summary>Gets or sets the SSL identifier (sslId).</summary>
    public int SslId { get; set; }

    /// <summary>Gets or sets the common name.</summary>
    public string? CommonName { get; set; }

    /// <summary>Gets or sets the certificate serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the subject alternative names.</summary>
    public IReadOnlyList<string>? SubjectAlternativeNames { get; set; }

    /// <summary>Gets or sets external requester information, when provided.</summary>
    public string? ExternalRequester { get; set; }

    /// <summary>Gets or sets the legacy order number, when returned by the API.</summary>
    public long OrderNumber { get; set; }

    /// <summary>Gets or sets the organization identifier, when returned by the API.</summary>
    public int OrgId { get; set; }

    /// <summary>Gets or sets the certificate status, when returned by the API.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the requester, when returned by the API.</summary>
    public string? Requester { get; set; }

    /// <summary>Gets or sets the owner, when returned by the API.</summary>
    public string? Owner { get; set; }

    /// <summary>Gets or sets the vendor, when returned by the API.</summary>
    public string? Vendor { get; set; }

    /// <summary>Gets or sets the term in days, when returned by the API.</summary>
    public int Term { get; set; }

    /// <summary>Gets or sets requested timestamp text, when returned by the API.</summary>
    public string? Requested { get; set; }

    /// <summary>Gets or sets expiry timestamp text, when returned by the API.</summary>
    public string? Expires { get; set; }

    /// <summary>Gets or sets suspend-notification state when available; null when not provided.</summary>
    public bool? SuspendNotifications { get; set; }
}
