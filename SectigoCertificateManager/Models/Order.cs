namespace SectigoCertificateManager.Models;

using SectigoCertificateManager;

/// <summary>
/// Represents a certificate order.
/// </summary>
public sealed class Order {
    /// <summary>Gets or sets the order identifier.</summary>
    public int Id { get; set; }

    /// <summary>Gets or sets the status of the order.</summary>
    public OrderStatus Status { get; set; }

    /// <summary>Gets or sets the backend certificate identifier.</summary>
    public string BackendCertId { get; set; } = string.Empty;

    /// <summary>Gets or sets the order number.</summary>
    public int OrderNumber { get; set; }
}