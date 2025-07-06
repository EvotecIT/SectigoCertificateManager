using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates client-side validation failures.
/// </summary>
public static class ValidationExample {
    public static void Run() {
        var request = new IssueCertificateRequest();
        try {
            RequestValidator.Validate(request);
        } catch (System.ComponentModel.DataAnnotations.ValidationException ex) {
            Console.WriteLine($"Validation failed: {ex.Message}");
        }
    }
}
