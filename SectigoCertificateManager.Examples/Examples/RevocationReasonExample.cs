using SectigoCertificateManager;

namespace SectigoCertificateManager.Examples.Examples;

public static class RevocationReasonExample {
    /// <summary>
    /// Demonstrates retrieving a revocation reason description.
    /// </summary>
    public static void Run() {
        const RevocationReason code = RevocationReason.Superseded;
        var description = RevocationReasons.GetRevocationReasonDescription(code);
        Console.WriteLine($"Reason {(int)code}: {description}");
    }
}
