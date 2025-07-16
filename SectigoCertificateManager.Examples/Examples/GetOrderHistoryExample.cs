using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates retrieving history entries for an order.
/// </summary>
public static class GetOrderHistoryExample {
    /// <summary>
    /// Executes the example that fetches order history.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var orders = new OrdersClient(client);

        var history = await orders.GetHistoryAsync(12345);
        foreach (var entry in history) {
            Console.WriteLine($"{entry.Date:u} - {entry.Event}");
        }
    }
}
