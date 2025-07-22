namespace SectigoCertificateManager.Models;

/// <summary>
/// Specifies action for administrators related to a template.
/// </summary>
public enum RelatedAdminsAction {
    /// <summary>Unlink administrators from the template.</summary>
    Unlink,

    /// <summary>Delete administrators with the template.</summary>
    Delete
}
