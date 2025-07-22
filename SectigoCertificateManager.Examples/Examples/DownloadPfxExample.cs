using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates downloading a certificate as PFX.
/// </summary>
public static class DownloadPfxExample {
    /// <summary>
    /// Executes the example that downloads a PFX file.
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

        Console.WriteLine("Downloading PFX...");
        await certificates.DownloadAsync(12345, "certificate.pfx", format: "pfx", password: "secret", progress: progress);
    }
}
