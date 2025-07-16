namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload used to revoke a certificate.
/// </summary>
public sealed class RevokeCertificateRequest {
    /// <summary>Gets or sets the certificate identifier.</summary>
    public int? CertId { get; set; }

    /// <summary>Gets or sets the certificate serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the certificate issuer.</summary>
    public string? Issuer { get; set; }

    /// <summary>Gets or sets the revocation date.</summary>
    public DateTimeOffset? RevokeDate { get; set; }

    /// <summary>Gets or sets the revocation reason code.</summary>
    public int ReasonCode { get; set; }

    /// <summary>Gets or sets the revocation reason message.</summary>
    public string? Reason { get; set; }
}