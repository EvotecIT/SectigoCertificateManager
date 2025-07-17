namespace SectigoCertificateManager.Responses;

/// <summary>
/// Represents a response from the certificate validation endpoint.
/// </summary>
public sealed class ValidateCertificateResponse {
    /// <summary>Gets or sets a value indicating whether the request is valid.</summary>
    public bool IsValid { get; set; }
}
