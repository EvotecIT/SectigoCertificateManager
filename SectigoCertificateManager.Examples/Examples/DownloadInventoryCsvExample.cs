using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

public static class DownloadInventoryCsvExample {
    /// <summary>
    /// Demonstrates downloading certificate inventory as CSV.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .Build();

        var client = new SectigoClient(config);
        var inventory = new InventoryClient(client);

        var request = new InventoryCsvRequest { Size = 10 };
        var records = await inventory.DownloadCsvAsync(request);
        Console.WriteLine($"Downloaded {records.Count} records.");
    }
}
