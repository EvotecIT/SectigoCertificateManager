namespace SectigoCertificateManager;

/// <summary>
/// Enumerates statuses for certificate orders.
/// </summary>
public enum OrderStatus
{
    /// <summary>Order is not initiated.</summary>
    NotInitiated,

    /// <summary>Order was submitted.</summary>
    Submitted,

    /// <summary>Order was completed.</summary>
    Completed,

    /// <summary>Order was cancelled.</summary>
    Cancelled
}
