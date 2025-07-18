using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Utilities;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates parsing the AuthorityInfoAccess extension.
/// </summary>
public static class AuthorityInfoAccessExample
{
    /// <summary>
    /// Executes the example showing how to read OCSP and CA issuer URLs.
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

        Console.WriteLine("Downloading certificate...");
        using var certificate = await certificates.DownloadAsync(12345);

        var aia = certificate.GetAuthorityInfoAccess();
        foreach (var url in aia.OcspUris)
        {
            Console.WriteLine($"OCSP: {url}");
        }
        foreach (var url in aia.CaIssuerUris)
        {
            Console.WriteLine($"CA Issuer: {url}");
        }
    }
}
