namespace SectigoCertificateManager.Requests;

using System;
using System.Collections.Generic;

/// <summary>
/// Request payload used to replace an SSL certificate via the Admin API.
/// </summary>
public sealed class AdminSslReplaceRequest {
    /// <summary>
    /// Gets or sets the certificate signing request.
    /// </summary>
    public string Csr { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a short message describing why the certificate needs to be replaced.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the certificate common name.
    /// </summary>
    public string? CommonName { get; set; }

    /// <summary>
    /// Gets or sets subject alternative names.
    /// </summary>
    public IReadOnlyList<string>? SubjectAlternativeNames { get; set; }

    /// <summary>
    /// Gets or sets the DCV mode for domain validation.
    /// </summary>
    public DcvMode DcvMode { get; set; }

    /// <summary>
    /// Gets or sets the DCV email address, when email validation is used.
    /// </summary>
    public string? DcvEmail { get; set; }
}
