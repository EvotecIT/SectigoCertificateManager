namespace SectigoCertificateManager.Responses;

using SectigoCertificateManager.Models;

/// <summary>
/// Represents a single result entry returned from the Admin Operations certificate import endpoint.
/// </summary>
public sealed class CertificateImportResult {
    /// <summary>Gets or sets a value indicating whether the import was successful.</summary>
    public bool Successful { get; set; }

    /// <summary>Gets or sets the backend certificate identifier provided by the CA.</summary>
    public string? BackendCertId { get; set; }

    /// <summary>Gets or sets the identity of the imported certificate, when available.</summary>
    public CertificateIdentity? Cert { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the certificate was created during the import.
    /// When <c>false</c>, it indicates the certificate already existed.
    /// </summary>
    public bool Created { get; set; }

    /// <summary>Gets or sets an error message describing why the import failed, when unsuccessful.</summary>
    public string? ErrorMessage { get; set; }
}

