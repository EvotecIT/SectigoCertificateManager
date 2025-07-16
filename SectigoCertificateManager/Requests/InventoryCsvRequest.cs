namespace SectigoCertificateManager.Requests;

/// <summary>
/// Request describing inventory CSV filters.
/// </summary>
public sealed class InventoryCsvRequest {
    /// <summary>Gets or sets the number of results to return.</summary>
    public int? Size { get; set; }

    /// <summary>Gets or sets the result offset.</summary>
    public int? Position { get; set; }

    /// <summary>Gets or sets the starting date for the inventory.</summary>
    public DateTime? DateFrom { get; set; }

    /// <summary>Gets or sets the ending date for the inventory.</summary>
    public DateTime? DateTo { get; set; }
}
