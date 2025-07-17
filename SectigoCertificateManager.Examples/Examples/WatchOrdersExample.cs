using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates polling order status until completion.
/// </summary>
public static class WatchOrdersExample {
    /// <summary>Executes the example.</summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var statuses = new OrderStatusClient(client);

        var result = await statuses.WatchAsync(12345, TimeSpan.FromSeconds(5));
        Console.WriteLine($"Final status: {result}");
    }
}
