using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates creating orders from request objects.
/// </summary>
public static class CreateOrdersExample {
    /// <summary>Executes the example.</summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var orders = new OrdersClient(client);

        var requests = new[] {
            new CreateOrderRequest { ProfileId = 1, Csr = "<csr1>" },
            new CreateOrderRequest { ProfileId = 2, Csr = "<csr2>" }
        };

        var ids = await orders.CreateAsync(requests);
        foreach (var id in ids) {
            Console.WriteLine($"Created order {id}");
        }
    }
}