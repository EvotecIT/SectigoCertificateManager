using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates streaming certificates using <see cref="CertificatesClient"/> search.
/// </summary>
public static class StreamCertificatesSearchExample {
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
        var request = new CertificateSearchRequest { CommonName = "example.com" };

        await foreach (var certificate in certificates.EnumerateSearchAsync(request)) {
            Console.WriteLine($"Certificate ID: {certificate.Id}");
        }
    }
}
