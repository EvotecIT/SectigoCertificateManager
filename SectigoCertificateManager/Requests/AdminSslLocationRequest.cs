namespace SectigoCertificateManager.Requests;

using System.Collections.Generic;

/// <summary>
/// Request payload used to create or update a certificate location.
/// </summary>
public sealed class AdminSslLocationRequest {
    /// <summary>
    /// Gets or sets key-value details for the certificate location.
    /// </summary>
    public IDictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
}

