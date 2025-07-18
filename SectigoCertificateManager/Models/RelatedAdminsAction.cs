namespace SectigoCertificateManager.Models;

using System.Text.Json.Serialization;

/// <summary>
/// Specifies action for administrators related to a template.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum RelatedAdminsAction {
    /// <summary>Unlink administrators from the template.</summary>
    Unlink,

    /// <summary>Delete administrators with the template.</summary>
    Delete
}
