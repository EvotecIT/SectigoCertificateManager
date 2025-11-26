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
}

