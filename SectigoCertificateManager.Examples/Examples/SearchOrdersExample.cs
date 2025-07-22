using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates order search.
/// </summary>
public static class SearchOrdersExample {
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

        var request = new OrderSearchRequest { Size = 10 };
        var result = await orders.SearchAsync(request);
        Console.WriteLine($"Found {result?.Orders.Count ?? 0} orders");
    }
}
