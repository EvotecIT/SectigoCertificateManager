namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload used to validate a certificate request.
/// </summary>
public sealed class ValidateCertificateRequest {
    /// <summary>Gets or sets the certificate signing request.</summary>
    public string? Csr { get; set; }
}
