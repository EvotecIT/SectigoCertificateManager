namespace SectigoCertificateManager.Requests;

using System.Collections.Generic;

/// <summary>
/// Request used to create a new order.
/// </summary>
public sealed class CreateOrderRequest {
    /// <summary>Gets or sets the identifier of the profile to use.</summary>
    public int ProfileId { get; set; }

    /// <summary>Gets or sets the certificate signing request.</summary>
    public string Csr { get; set; } = string.Empty;

    /// <summary>Gets or sets subject alternative names.</summary>
    public IReadOnlyList<string> SubjectAlternativeNames { get; set; } = [];
}