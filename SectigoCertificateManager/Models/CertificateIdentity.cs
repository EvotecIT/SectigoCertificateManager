namespace SectigoCertificateManager.Models;

/// <summary>
/// Represents a minimal certificate identity used by Admin Operations import responses.
/// </summary>
public sealed class CertificateIdentity {
    /// <summary>Gets or sets the certificate identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the certificate subject.</summary>
    public string? Subject { get; set; }

    /// <summary>Gets or sets the certificate serial number.</summary>
    public string? SerialNumber { get; set; }
}

