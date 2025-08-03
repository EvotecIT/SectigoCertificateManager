using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates incremental synchronization of orders.
/// </summary>
public static class IncrementalOrderSyncExample {
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

        var lastSync = DateTime.UtcNow.AddDays(-1);
        var request = new OrderSearchRequest {
            Size = 10,
            UpdatedAfter = lastSync
        };

        await foreach (var order in orders.EnumerateSearchAsync(request)) {
            Console.WriteLine($"Processing order {order.Id}");
        }

        lastSync = DateTime.UtcNow;
        Console.WriteLine($"Next sync should use {lastSync:s}");
    }
}

