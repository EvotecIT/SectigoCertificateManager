namespace SectigoCertificateManager.Models;

using System.Collections.Generic;
using SectigoCertificateManager;

public sealed class Certificate
{
    public int Id { get; set; }

    public string? CommonName { get; set; }

    public int OrgId { get; set; }

    public CertificateStatus Status { get; set; } = CertificateStatus.Any;

    public long OrderNumber { get; set; }

    public string BackendCertId { get; set; } = string.Empty;

    public string? Vendor { get; set; }

    public Profile? CertType { get; set; }

    public int Term { get; set; }

    public string? Owner { get; set; }

    public string? Requester { get; set; }

    public string? Comments { get; set; }

    public string? Requested { get; set; }

    public string? Expires { get; set; }

    public string? SerialNumber { get; set; }

    public string? KeyAlgorithm { get; set; }

    public int? KeySize { get; set; }

    public string? KeyType { get; set; }

    public IReadOnlyList<string> SubjectAlternativeNames { get; set; } = [];

    public bool SuspendNotifications { get; set; }
}
