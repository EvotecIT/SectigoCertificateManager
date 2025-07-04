using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates paging through certificate results.
/// </summary>
public static class PagingExample
{
    public static async Task RunAsync()
    {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_5)
            .Build();

        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);

        await foreach (var certificate in certificates.ListAsync())
        {
            Console.WriteLine($"Certificate ID: {certificate.Id}");
        }
    }
}
