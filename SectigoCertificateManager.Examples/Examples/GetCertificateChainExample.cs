using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Security.Cryptography.X509Certificates;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates retrieving a certificate chain.
/// </summary>
public static class GetCertificateChainExample {
    /// <summary>
    /// Executes the example that retrieves a certificate chain.
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

        Console.WriteLine("Retrieving certificate chain...");
        using var chain = await certificates.GetChainAsync(12345);
        foreach (var element in chain.ChainElements) {
            Console.WriteLine(element.Certificate.Subject);
        }
    }
}
