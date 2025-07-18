namespace SectigoCertificateManager.Requests;

using System.Collections.Generic;
using SectigoCertificateManager.Models;

/// <summary>
/// Request payload used to create an IdP template.
/// </summary>
public sealed class CreateIdpTemplateRequest {
    /// <summary>Gets or sets the template name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Gets or sets assigned privileges.</summary>
    public IReadOnlyList<string> Privileges { get; set; } = [];

    /// <summary>Gets or sets credentials to assign.</summary>
    public IReadOnlyList<IdpTemplateCredential> Credentials { get; set; } = [];

    /// <summary>Gets or sets the identity provider identifier.</summary>
    public int IdentityProviderId { get; set; }

    /// <summary>Gets or sets attribute mapping rules.</summary>
    public IReadOnlyList<IdpMappingRule> IdpMappingRules { get; set; } = [];
}
