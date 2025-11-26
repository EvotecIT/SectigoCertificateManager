namespace SectigoCertificateManager.AdminApi;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents a simple identifier/name pair used in validation responses.
/// </summary>
public sealed class AdminIdName {
    /// <summary>Gets or sets the identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the name.</summary>
    public string? Name { get; set; }
}

/// <summary>
/// Represents a summary entry for an organization validation.
/// </summary>
public sealed class AdminValidationSummary {
    /// <summary>Gets or sets the validation identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the validation type (EMAIL, DV, OV, EV, PRIVATE).</summary>
    public string? Type { get; set; }

    /// <summary>Gets or sets the validation status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the background validation status.</summary>
    public string? BackgroundStatus { get; set; }

    /// <summary>Gets or sets the submitted date.</summary>
    public string? Submitted { get; set; }

    /// <summary>Gets or sets the expiration date.</summary>
    public string? Expires { get; set; }

    /// <summary>Gets or sets a value indicating whether this is an alternative validation set.</summary>
    public bool Alternative { get; set; }

    /// <summary>Gets or sets the backend identifier.</summary>
    public int? BackendId { get; set; }

    /// <summary>Gets or sets the backend type (SASP, PRIVATE_CA, etc.).</summary>
    public string? BackendType { get; set; }

    /// <summary>Gets or sets the validated domain.</summary>
    public string? Domain { get; set; }

    /// <summary>Gets or sets arbitrary validation settings.</summary>
    public IReadOnlyDictionary<string, string>? Settings { get; set; }

    /// <summary>Gets or sets the validator administrator.</summary>
    public AdminIdName? Validator { get; set; }

    /// <summary>Gets or sets the permitted certificate types for this validation.</summary>
    public IReadOnlyList<string> PermittedCertTypes { get; set; } = Array.Empty<string>();
}

/// <summary>
/// Represents detailed information about an organization validation.
/// </summary>
public sealed class AdminValidationDetails {
    /// <summary>Gets or sets the validation identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the validation level (OV_SSL, OV_SMIME, EV_SSL).</summary>
    public string? ValidationLevel { get; set; }

    /// <summary>Gets or sets the validation status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets the background validation status.</summary>
    public string? BackgroundStatus { get; set; }

    /// <summary>Gets or sets the submitted date.</summary>
    public string? Submitted { get; set; }

    /// <summary>Gets or sets the background submitted date.</summary>
    public string? BackgroundSubmitted { get; set; }

    /// <summary>Gets or sets the expiration date.</summary>
    public string? Expires { get; set; }

    /// <summary>Gets or sets a value indicating whether this is an alternative validation set.</summary>
    public bool Alternative { get; set; }

    /// <summary>Gets or sets the backend identifier.</summary>
    public int? BackendId { get; set; }

    /// <summary>Gets or sets the backend type.</summary>
    public string? BackendType { get; set; }

    /// <summary>Gets or sets validation settings as key/value pairs.</summary>
    public IReadOnlyDictionary<string, string>? Settings { get; set; }

    /// <summary>Gets or sets the validator administrator.</summary>
    public AdminIdName? Validator { get; set; }
}

