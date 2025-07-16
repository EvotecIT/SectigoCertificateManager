using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates issuing a certificate using <see cref="IssueCertificateRequestBuilder"/>.
/// </summary>
public static class IssueCertificateWithBuilderExample {
    /// <summary>
    /// Executes the example that issues a certificate.
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

        var profile = new Profile { Id = 1, Terms = new[] { 12, 24 } };

        var request = new IssueCertificateRequestBuilder(profile.Terms)
            .WithCommonName("example.com")
            .WithProfileId(profile.Id)
            .WithTerm(12)
            .Build();

        var result = await certificates.IssueAsync(request);
        Console.WriteLine($"New certificate id: {result?.Id}");
    }
}
