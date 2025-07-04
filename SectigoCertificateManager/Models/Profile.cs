namespace SectigoCertificateManager.Models;

using System.Collections.Generic;
/// <summary>
/// Represents a certificate profile.
/// </summary>
public sealed class Profile {
    /// <summary>Gets or sets the profile identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the profile name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the description.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets allowed certificate terms.</summary>
    public IReadOnlyList<int> Terms { get; set; } = [];

    /// <summary>Gets or sets supported key types.</summary>
    public IReadOnlyDictionary<string, IReadOnlyList<string>> KeyTypes { get; set; } = new Dictionary<string, IReadOnlyList<string>>();

    /// <summary>Gets or sets a value indicating whether secondary organization names can be used.</summary>
    public bool UseSecondaryOrgName { get; set; }
}