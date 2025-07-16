using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates configuring the API client and retrieving a certificate.
/// Credentials are generated in the Sectigo Certificate Manager portal.
/// Navigate to <strong>Administration</strong> \u2192 <strong>Users</strong>, edit your user
/// and create API credentials. Use the generated login and password below.
/// </summary>
public static class BasicApiExample {
    /// <summary>
    /// Executes the example showing basic API usage.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .WithConcurrencyLimit(2)
            .Build();

        var factory = new SectigoClientFactory();
        var client = factory.Create(config);
        var certificates = new CertificatesClient(client);

        Console.WriteLine("Requesting certificate details...");
        var certificate = await certificates.GetAsync(12345);
        Console.WriteLine($"Common name: {certificate?.CommonName}");
    }
}