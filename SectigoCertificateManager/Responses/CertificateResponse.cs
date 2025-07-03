namespace SectigoCertificateManager.Responses;

using SectigoCertificateManager.Models;
using System.Collections.Generic;

/// <summary>
/// Represents a response containing certificates.
/// </summary>
public sealed class CertificateResponse
{
    /// <summary>
    /// Gets or sets certificates returned by the API.
    /// </summary>
    public IReadOnlyList<Certificate> Certificates { get; set; } = [];
}
