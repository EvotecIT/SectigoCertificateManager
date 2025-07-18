namespace SectigoCertificateManager.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Specifies the match type used in IdP mapping rules.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum MatchType {
    /// <summary>Attribute value must match completely.</summary>
    Matches,

    /// <summary>Attribute value must contain the specified text.</summary>
    Contains
}
