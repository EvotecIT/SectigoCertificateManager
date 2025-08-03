using System;
using SectigoCertificateManager.Utilities;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates validating certificate signing requests.
/// </summary>
public static class CsrValidatorExample {
    /// <summary>Executes the example.</summary>
    public static void Run() {
        var (csr, key) = CsrGenerator.GenerateRsa("CN=example.com");
        var valid = CsrValidator.IsValid(csr);
        Console.WriteLine($"CSR valid: {valid}");
        key.Dispose();
    }
}
