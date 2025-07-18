using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates downloading an issued certificate as an X509Certificate2 instance.
/// </summary>
public static class DownloadCertificateObjectExample {
    /// <summary>
    /// Executes the example that downloads a certificate as an object.
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

        Console.WriteLine("Downloading certificate...");
        using var cert = await certificates.DownloadAsync(12345);
        Console.WriteLine($"Thumbprint: {cert.Thumbprint}");
    }
}
