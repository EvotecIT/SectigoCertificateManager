using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates renewing a certificate using the API.
/// </summary>
public static class RenewCertificateExample {
    /// <summary>
    /// Executes the example that renews an existing certificate.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var factory = new SectigoClientFactory();
        var client = factory.Create(config);
        var certificates = new CertificatesClient(client);

        Console.WriteLine("Renewing certificate...");
        var request = new RenewCertificateRequest {
            Csr = "<csr>",
            DcvMode = "EMAIL",
            DcvEmail = "admin@example.com"
        };
        var newId = await certificates.RenewAsync(12345, request);
        Console.WriteLine($"New certificate id: {newId}");
    }
}