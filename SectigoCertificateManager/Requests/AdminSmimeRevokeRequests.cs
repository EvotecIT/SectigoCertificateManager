namespace SectigoCertificateManager.Requests;

using System;

/// <summary>
/// Request payload used when revoking a S/MIME certificate by backend identifier or serial number.
/// </summary>
public sealed class AdminSmimeRevokeRequest {
    /// <summary>
    /// Gets or sets the revocation reason code (for example, "0", "1", "3", "4", "5").
    /// </summary>
    public string? ReasonCode { get; set; }

    /// <summary>
    /// Gets or sets a short message explaining why the certificate needs to be revoked.
    /// </summary>
    public string? Reason { get; set; }
}

/// <summary>
/// Request payload used when revoking S/MIME certificates by email.
/// </summary>
public sealed class AdminSmimeRevokeByEmailRequest {
    /// <summary>
    /// Gets or sets the revocation reason code (for example, "0", "1", "3", "4", "5").
    /// </summary>
    public string? ReasonCode { get; set; }

    /// <summary>
    /// Gets or sets a short message explaining why the certificate needs to be revoked.
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Gets or sets the email address whose certificates should be revoked.
    /// </summary>
    public string Email { get; set; } = string.Empty;
}

/// <summary>
/// Request payload used when marking a S/MIME certificate as revoked in SCM.
/// </summary>
public sealed class AdminSmimeMarkAsRevokedRequest {
    /// <summary>Gets or sets the certificate identifier.</summary>
    public int? CertId { get; set; }

    /// <summary>Gets or sets the certificate serial number.</summary>
    public string? SerialNumber { get; set; }

    /// <summary>Gets or sets the certificate issuer.</summary>
    public string? Issuer { get; set; }

    /// <summary>Gets or sets the revocation date.</summary>
    public DateTimeOffset? RevokeDate { get; set; }

    /// <summary>
    /// Gets or sets the revocation reason code (for example, "0", "1", "3", "4", "5").
    /// </summary>
    public string? ReasonCode { get; set; }
}

