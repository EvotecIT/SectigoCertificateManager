using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates renewing a certificate for an order.
/// </summary>
public static class RenewOrderCertificateExample {
    /// <summary>Executes the example that renews an order's certificate.</summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var orders = new OrdersClient(client);

        Console.WriteLine("Renewing certificate for order...");
        var request = new RenewCertificateRequest {
            Csr = "<csr>",
            DcvMode = "EMAIL",
            DcvEmail = "admin@example.com"
        };
        var newId = await orders.RenewCertificateAsync(12345, request);
        Console.WriteLine($"New certificate id: {newId}");
    }
}

