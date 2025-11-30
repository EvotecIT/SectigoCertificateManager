namespace SectigoCertificateManager.Requests;

using System.Collections.Generic;
using SectigoCertificateManager.Models;

/// <summary>
/// Request payload used to update an IdP template.
/// </summary>
public sealed class UpdateIdpTemplateRequest {
    /// <summary>Display name of the template.</summary>
    public string? Name { get; set; }
    /// <summary>Privileges to assign to administrators using this template.</summary>
    public IReadOnlyList<string>? Privileges { get; set; }
    /// <summary>Role/organization credentials mapped to the template.</summary>
    public IReadOnlyList<IdpTemplateCredential>? Credentials { get; set; }
    /// <summary>Identifier of the associated identity provider.</summary>
    public int? IdentityProviderId { get; set; }
    /// <summary>Optional mapping rules supplied by the IdP.</summary>
    public IReadOnlyList<IdpMappingRule>? IdpMappingRules { get; set; }
}
