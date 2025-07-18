namespace SectigoCertificateManager.Models;

using System.Collections.Generic;

/// <summary>
/// Represents an IdP attribute mapping rule.
/// </summary>
public sealed class IdpMappingRule {
    /// <summary>Gets or sets the attribute name.</summary>
    public string Attribute { get; set; } = string.Empty;

    /// <summary>Gets or sets the match type.</summary>
    public MatchType MatchType { get; set; } = MatchType.Matches;

    /// <summary>Gets or sets allowed values.</summary>
    public IReadOnlyList<string> Values { get; set; } = [];
}
