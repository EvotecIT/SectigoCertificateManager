using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates uploading a CSV file containing multiple orders.
/// </summary>
public static class UploadOrdersExample {
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

        using var stream = File.OpenRead("orders.csv");
        var progress = new Progress<double>(p => Console.WriteLine($"Uploaded {p:P0}"));
        await orders.UploadAsync(stream, "text/csv", progress);
    }
}
