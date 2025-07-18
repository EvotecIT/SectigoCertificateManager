using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Models;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates creating or updating a certificate type using <see cref="CertificateTypesClient"/>.
/// </summary>
public static class UpsertCertificateTypeExample {
    /// <summary>
    /// Executes the example that upserts a certificate type.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var types = new CertificateTypesClient(client);

        var type = new CertificateType { Name = "My Custom Type" };
        var result = await types.UpsertAsync(type);
        Console.WriteLine($"Created certificate type with ID: {result?.Id}");
    }
}
