namespace SectigoCertificateManager;

using System.Collections.Generic;

/// <summary>
/// Provides descriptions for revocation reason codes.
/// </summary>
public static class RevocationReasons {
    private static readonly IReadOnlyDictionary<RevocationReason, string> s_descriptions = new Dictionary<RevocationReason, string> {
        [RevocationReason.Unspecified] = "unspecified",
        [RevocationReason.KeyCompromise] = "keyCompromise",
        [RevocationReason.AffiliationChanged] = "affiliationChanged",
        [RevocationReason.Superseded] = "superseded",
        [RevocationReason.CessationOfOperation] = "cessationOfOperation"
    };

    /// <summary>Gets the description associated with a revocation reason code.</summary>
    /// <param name="code">Revocation reason code.</param>
    /// <returns>The matching description, or <c>null</c> if the code is unknown.</returns>
    public static string? GetRevocationReasonDescription(RevocationReason code) =>
        s_descriptions.TryGetValue(code, out var desc) ? desc : null;
}
