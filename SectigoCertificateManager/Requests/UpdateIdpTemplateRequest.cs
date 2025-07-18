namespace SectigoCertificateManager.Requests;

using System.Collections.Generic;
using SectigoCertificateManager.Models;

/// <summary>
/// Request payload used to update an IdP template.
/// </summary>
public sealed class UpdateIdpTemplateRequest {
    public string? Name { get; set; }
    public IReadOnlyList<string>? Privileges { get; set; }
    public IReadOnlyList<IdpTemplateCredential>? Credentials { get; set; }
    public int? IdentityProviderId { get; set; }
    public IReadOnlyList<IdpMappingRule>? IdpMappingRules { get; set; }
}
