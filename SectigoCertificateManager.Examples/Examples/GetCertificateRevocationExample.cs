using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates retrieving revocation details for a certificate.
/// </summary>
public static class GetCertificateRevocationExample
{
    /// <summary>
    /// Executes the example that retrieves certificate revocation details.
    /// </summary>
    public static async Task RunAsync()
    {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);

        Console.WriteLine("Requesting certificate revocation info...");
        var revocation = await certificates.GetRevocationAsync(12345);
        Console.WriteLine($"Revocation reason: {revocation?.Reason}");
    }
}
