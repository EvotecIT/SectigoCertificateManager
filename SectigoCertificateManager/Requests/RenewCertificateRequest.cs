namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload used to renew a certificate.
/// </summary>
public sealed class RenewCertificateRequest {
    /// <summary>Gets or sets the certificate signing request.</summary>
    public string? Csr { get; set; }

    /// <summary>Gets or sets the DCV mode.</summary>
    public string? DcvMode { get; set; }

    /// <summary>Gets or sets the DCV email.</summary>
    public string? DcvEmail { get; set; }
}
