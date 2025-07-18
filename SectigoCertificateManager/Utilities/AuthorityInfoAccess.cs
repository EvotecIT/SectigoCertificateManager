namespace SectigoCertificateManager.Utilities;

using System.Collections.Generic;

/// <summary>
/// Represents the AuthorityInfoAccess extension data.
/// </summary>
public sealed class AuthorityInfoAccess
{
    /// <summary>Gets OCSP URLs from the extension.</summary>
    public IReadOnlyList<string> OcspUris { get; set; } = System.Array.Empty<string>();

    /// <summary>Gets CA issuer URLs from the extension.</summary>
    public IReadOnlyList<string> CaIssuerUris { get; set; } = System.Array.Empty<string>();
}
