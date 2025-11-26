namespace SectigoCertificateManager.Requests;

using SectigoCertificateManager.AdminApi;
using System.Collections.Generic;

/// <summary>
/// Request payload used to create a global custom field via <c>/api/customField/v2</c>.
/// </summary>
public sealed class CreateCustomFieldV2Request {
    /// <summary>Gets or sets the custom field name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets the certificate type associated with this custom field (for example, SSL, SMIME, Device, CodeSign).</summary>
    public string CertType { get; set; } = string.Empty;

    /// <summary>Gets or sets the input configuration.</summary>
    public AdminCustomFieldInput Input { get; set; } = new AdminCustomFieldInput();

    /// <summary>Gets or sets access methods where the custom field is mandatory.</summary>
    public IReadOnlyList<string>? Mandatories { get; set; }
}

