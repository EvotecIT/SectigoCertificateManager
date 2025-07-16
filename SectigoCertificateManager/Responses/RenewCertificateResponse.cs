namespace SectigoCertificateManager.Responses;

/// <summary>
/// Represents a response from the certificate renew endpoint.
/// </summary>
public sealed class RenewCertificateResponse {
    /// <summary>Gets or sets the new certificate identifier.</summary>
    public int SslId { get; set; }
}