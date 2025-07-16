using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates importing certificates from a zip archive.
/// </summary>
public static class ImportCertificatesExample {
    /// <summary>Executes the certificate import example.</summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);

        Console.WriteLine("Importing certificates...");
        using var stream = File.OpenRead("certs.zip");
        var result = await certificates.ImportAsync(123, stream, "certs.zip");
        Console.WriteLine($"Processed {result?.ProcessedCount ?? 0} certificates.");
    }
}
