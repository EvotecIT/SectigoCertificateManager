namespace SectigoCertificateManager.Models;

/// <summary>
/// Specifies the match type used in IdP mapping rules.
/// </summary>
public enum MatchType {
    /// <summary>Attribute value must match completely.</summary>
    Matches,

    /// <summary>Attribute value must contain the specified text.</summary>
    Contains
}
