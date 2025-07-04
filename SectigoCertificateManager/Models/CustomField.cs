namespace SectigoCertificateManager.Models;

/// <summary>
/// Represents a custom field value.
/// </summary>
public sealed class CustomField
{
    /// <summary>Gets or sets the field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the field value.</summary>
    public string Value { get; set; } = string.Empty;
}
