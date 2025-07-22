namespace SectigoCertificateManager.Requests;

/// <summary>
/// Filter parameters for searching orders.
/// </summary>
public sealed class OrderSearchRequest {
    /// <summary>Gets or sets the result offset.</summary>
    public int? Position { get; set; }

    /// <summary>Gets or sets the number of results to return.</summary>
    public int? Size { get; set; }

    /// <summary>Gets or sets the order status filter.</summary>
    public OrderStatus? Status { get; set; }

    /// <summary>Gets or sets the order number filter.</summary>
    public int? OrderNumber { get; set; }

    /// <summary>Gets or sets the backend certificate identifier filter.</summary>
    public string? BackendCertId { get; set; }
}
