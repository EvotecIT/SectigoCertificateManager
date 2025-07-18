using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates downloading an issued certificate.
/// </summary>
public static class DownloadCertificateExample {
    /// <summary>
    /// Executes the example that downloads a certificate to disk.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);
        var progress = new Progress<double>(p => Console.WriteLine($"Downloaded {p:P0}"));

        Console.WriteLine("Downloading certificate...");
        await certificates.DownloadAsync(12345, "certificate.p7b", progress: progress);
    }
}