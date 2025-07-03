namespace SectigoCertificateManager.Responses;

using SectigoCertificateManager.Models;

/// <summary>
/// Represents a response containing a certificate order.
/// </summary>
public sealed class OrderResponse
{
    /// <summary>
    /// Gets or sets the order information.
    /// </summary>
    public Order? Order { get; set; }
}
