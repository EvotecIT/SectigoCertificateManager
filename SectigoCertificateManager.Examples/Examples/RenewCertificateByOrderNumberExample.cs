using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates renewing a certificate using an order number.
/// </summary>
public static class RenewCertificateByOrderNumberExample {
    /// <summary>Executes the example that renews a certificate by order number.</summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);

        Console.WriteLine("Renewing certificate by order number...");
        var request = new RenewCertificateRequest {
            Csr = "<csr>",
            DcvMode = "EMAIL",
            DcvEmail = "admin@example.com"
        };
        var newId = await certificates.RenewByOrderNumberAsync(12345, request);
        Console.WriteLine($"New certificate id: {newId}");
    }
}

