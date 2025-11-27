namespace SectigoCertificateManager.Models;

using System.Collections.Generic;

/// <summary>
/// Represents a certificate location in the Admin API.
/// </summary>
public sealed class AdminSslLocation {
    /// <summary>Gets or sets the location identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the location type.</summary>
    public string LocationType { get; set; } = string.Empty;

    /// <summary>Gets or sets the location name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets key-value details for the certificate location.</summary>
    public IReadOnlyDictionary<string, string> Details { get; set; } = new Dictionary<string, string>();
}

