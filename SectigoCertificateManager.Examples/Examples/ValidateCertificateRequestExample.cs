using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Responses;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates validating a certificate request.
/// </summary>
public static class ValidateCertificateRequestExample {
    /// <summary>
    /// Executes the example that validates a certificate request.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);

        Console.WriteLine("Validating certificate request...");
        var request = new ValidateCertificateRequest { Csr = "<csr>" };
        var result = await certificates.ValidateCertificateRequestAsync(request);
        Console.WriteLine($"Is valid: {result?.IsValid}");
    }
}
