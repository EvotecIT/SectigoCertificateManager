namespace SectigoCertificateManager.Models;

public sealed class Order
{
    public int Id { get; set; }
    public string Status { get; set; } = string.Empty;
    public string BackendCertId { get; set; } = string.Empty;
    public int OrderNumber { get; set; }
}
