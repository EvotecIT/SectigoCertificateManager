namespace SectigoCertificateManager;

using System.Collections.Generic;

/// <summary>
/// Provides descriptions for revocation reason codes.
/// </summary>
public static class RevocationReasons {
    private static readonly IReadOnlyDictionary<int, string> s_descriptions = new Dictionary<int, string> {
        [0] = "unspecified",
        [1] = "keyCompromise",
        [3] = "affiliationChanged",
        [4] = "superseded",
        [5] = "cessationOfOperation"
    };

    /// <summary>Gets the description associated with a revocation reason code.</summary>
    /// <param name="code">Revocation reason code.</param>
    /// <returns>The matching description, or <c>null</c> if the code is unknown.</returns>
    public static string? GetRevocationReasonDescription(int code) =>
        s_descriptions.TryGetValue(code, out var desc) ? desc : null;
}
