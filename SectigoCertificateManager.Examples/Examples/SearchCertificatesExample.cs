using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

public static class SearchCertificatesExample {
    /// <summary>
    /// Executes the example demonstrating certificate search.
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

        var request = new CertificateSearchRequest {
            CommonName = "example.com",
            DateFrom = DateTime.UtcNow.AddDays(-7),
            DateTo = DateTime.UtcNow
        };

        var result = await certificates.SearchAsync(request);
        Console.WriteLine($"Found {result?.Certificates.Count ?? 0} certificates");
    }
}
