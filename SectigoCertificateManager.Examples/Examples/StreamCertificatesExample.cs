using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates streaming certificates using <see cref="CertificatesClient"/>.
/// </summary>
public static class StreamCertificatesExample {
    /// <summary>
    /// Executes the example that streams certificate records.
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

        await foreach (var certificate in certificates.EnumerateCertificatesAsync(pageSize: 50)) {
            Console.WriteLine($"Certificate ID: {certificate.Id}");
        }
    }
}
