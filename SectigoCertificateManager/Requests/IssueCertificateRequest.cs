namespace SectigoCertificateManager.Requests;

using System;
using System.Collections.Generic;

/// <summary>
/// Request payload used when issuing a new certificate.
/// </summary>
public sealed class IssueCertificateRequest {
    /// <summary>
    /// Gets or sets the common name of the certificate.
    /// </summary>
    public string CommonName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the identifier of the profile to use.
    /// </summary>
    public int ProfileId { get; set; }

    /// <summary>
    /// Gets or sets the term of the certificate.
    /// </summary>
    private int _term;

    /// <summary>Gets or sets the term of the certificate.</summary>
    public int Term {
        get => _term;
        set {
            if (value <= 0) {
                throw new ArgumentOutOfRangeException(nameof(value));
            }

            _term = value;
        }
    }

    /// <summary>
    /// Gets or sets subject alternative names.
    /// </summary>
    public IReadOnlyList<string> SubjectAlternativeNames { get; set; } = [];
}