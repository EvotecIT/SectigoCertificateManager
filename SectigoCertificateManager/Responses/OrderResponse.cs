namespace SectigoCertificateManager.Responses;

using SectigoCertificateManager.Models;
using System.Collections.Generic;

/// <summary>
/// Represents a response containing orders.
/// </summary>
public sealed class OrderResponse {
    /// <summary>Gets or sets orders returned by the API.</summary>
    public IReadOnlyList<Order> Orders { get; set; } = [];
}
