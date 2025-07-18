namespace SectigoCertificateManager.Models;

using System.Collections.Generic;

/// <summary>
/// Represents a custom field definition.
/// </summary>
public sealed class CustomField {
    /// <summary>Gets or sets the custom field identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the custom field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets a value indicating whether the field is mandatory.</summary>
    public bool Mandatory { get; set; }

    /// <summary>Gets or sets the certificate type this field applies to.</summary>
    public string? CertType { get; set; }

    /// <summary>Gets or sets the field state.</summary>
    public string? State { get; set; }

    /// <summary>Gets or sets input properties for this field.</summary>
    public CustomFieldInput? Input { get; set; }
}

/// <summary>
/// Represents input configuration for a custom field.
/// </summary>
public sealed class CustomFieldInput {
    /// <summary>Gets or sets the input type.</summary>
    public string? Type { get; set; }

    /// <summary>Gets or sets allowed options for option fields.</summary>
    public IReadOnlyList<string>? Options { get; set; }
}
