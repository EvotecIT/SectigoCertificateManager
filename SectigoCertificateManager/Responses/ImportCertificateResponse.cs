namespace SectigoCertificateManager.Responses;

/// <summary>
/// Represents a response from the certificate import endpoint.
/// </summary>
public sealed class ImportCertificateResponse {
    /// <summary>Gets or sets the total number of processed certificates.</summary>
    public int ProcessedCount { get; set; }

    /// <summary>Gets or sets the list of errors returned by the API.</summary>
    public IReadOnlyList<string> Errors { get; set; } = [];
}
