using System;
using SectigoCertificateManager.Utilities;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates generating certificate signing requests.
/// </summary>
public static class CsrGeneratorExample {
    /// <summary>
    /// Executes the example generating RSA and ECDSA CSRs.
    /// </summary>
    public static void Run() {
        var (rsaCsr, rsaKey) = CsrGenerator.GenerateRsa("CN=example.com");
        Console.WriteLine($"RSA CSR:\n{rsaCsr}\n");
        rsaKey.Dispose();

        var (ecdsaCsr, ecdsaKey) = CsrGenerator.GenerateEcdsa("CN=example.com");
        Console.WriteLine($"ECDSA CSR:\n{ecdsaCsr}\n");
        ecdsaKey.Dispose();
    }
}
