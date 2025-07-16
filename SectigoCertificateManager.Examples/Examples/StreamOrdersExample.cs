using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates streaming orders using <see cref="OrdersClient"/>.
/// </summary>
public static class StreamOrdersExample {
    /// <summary>
    /// Executes the example that streams order records.
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

        await foreach (var order in orders.EnumerateOrdersAsync(pageSize: 50)) {
            Console.WriteLine($"Order ID: {order.Id}");
        }
    }
}