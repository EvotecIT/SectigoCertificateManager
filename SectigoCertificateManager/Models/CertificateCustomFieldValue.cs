namespace SectigoCertificateManager.Models;

/// <summary>
/// A custom field value set on a certificate.
/// </summary>
public sealed class CertificateCustomFieldValue {
    /// <summary>Gets or sets the custom field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the value.</summary>
    public string? Value { get; set; }
}
