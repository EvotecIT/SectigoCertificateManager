namespace SectigoCertificateManager.AdminApi;

using System.Collections.Generic;

/// <summary>
/// Represents a global custom field definition returned by the Admin API (<c>ApiCustomFieldV2</c>).
/// </summary>
public sealed class AdminCustomFieldV2 {
    /// <summary>Gets or sets the custom field identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the custom field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the certificate type associated with this custom field.</summary>
    public string? CertType { get; set; }

    /// <summary>Gets or sets the custom field state.</summary>
    public string? State { get; set; }

    /// <summary>Gets or sets the input configuration for the custom field.</summary>
    public AdminCustomFieldInput? Input { get; set; }

    /// <summary>Gets or sets the access methods where the field is mandatory.</summary>
    public IReadOnlyList<string>? Mandatories { get; set; }
}

/// <summary>
/// Base input configuration for a custom field.
/// </summary>
public class AdminCustomFieldInput {
    /// <summary>Gets or sets the custom field input type.</summary>
    public string Type { get; set; } = string.Empty;
}

/// <summary>
/// Represents a text-option input with a fixed set of options.
/// </summary>
public sealed class AdminCustomFieldTextOptionInput : AdminCustomFieldInput {
    /// <summary>Gets or sets the available text options.</summary>
    public IReadOnlyList<string> Options { get; set; } = Array.Empty<string>();
}

