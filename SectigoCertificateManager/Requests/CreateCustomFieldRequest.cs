namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request payload used to create a custom field.
/// </summary>
public sealed class CreateCustomFieldRequest {
    /// <summary>Gets or sets the custom field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the field is mandatory.</summary>
    public bool Mandatory { get; set; }

    /// <summary>Gets or sets the certificate type this field applies to.</summary>
    public string CertType { get; set; } = string.Empty;

    /// <summary>Gets or sets the field state.</summary>
    public string State { get; set; } = string.Empty;
}
