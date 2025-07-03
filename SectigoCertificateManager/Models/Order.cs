namespace SectigoCertificateManager.Models;

using SectigoCertificateManager;

public sealed class Order
{
    public int Id { get; set; }
    public OrderStatus Status { get; set; }
    public string BackendCertId { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
}
