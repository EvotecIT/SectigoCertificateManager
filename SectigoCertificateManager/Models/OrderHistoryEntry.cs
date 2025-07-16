namespace SectigoCertificateManager.Models;

/// <summary>
/// Represents a single event in the lifecycle of an order.
/// </summary>
public sealed class OrderHistoryEntry {
    /// <summary>Gets or sets the timestamp of the event.</summary>
    public DateTimeOffset Date { get; set; }

    /// <summary>Gets or sets the textual description of the event.</summary>
    public string Event { get; set; } = string.Empty;

    /// <summary>Gets or sets the user responsible for the event.</summary>
    public string? PerformedBy { get; set; }
}
