namespace SectigoCertificateManager.Models;

/// <summary>
/// Represents an audit log entry.
/// </summary>
public sealed class AuditLogEntry {
    /// <summary>Gets or sets the entry identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the entry GUID.</summary>
    public string Guid { get; set; } = string.Empty;

    /// <summary>Gets or sets the access method used.</summary>
    public string AccessMethod { get; set; } = string.Empty;

    /// <summary>Gets or sets the action date.</summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>Gets or sets the source IP address.</summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>Gets or sets the description of the entry.</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Gets or sets the action information.</summary>
    public AuditLogAction? Action { get; set; }

    /// <summary>Gets or sets the administrator information.</summary>
    public AuditLogAdmin? Admin { get; set; }
}

/// <summary>
/// Represents information about the performed action.
/// </summary>
public sealed class AuditLogAction {
    /// <summary>Gets or sets the action identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the action name.</summary>
    public string ActionName { get; set; } = string.Empty;
}

/// <summary>
/// Represents information about the administrator who performed the action.
/// </summary>
public sealed class AuditLogAdmin {
    /// <summary>Gets or sets the admin login.</summary>
    public string Login { get; set; } = string.Empty;

    /// <summary>Gets or sets the admin full name.</summary>
    public string FullName { get; set; } = string.Empty;

    /// <summary>Gets or sets the admin email.</summary>
    public string Email { get; set; } = string.Empty;
}
