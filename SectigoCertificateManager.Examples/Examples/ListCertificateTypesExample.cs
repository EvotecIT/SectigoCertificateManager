using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates listing certificate types using <see cref="CertificateTypesClient"/>.
/// </summary>
public static class ListCertificateTypesExample {
    /// <summary>
    /// Executes the example that lists all certificate types.
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

        foreach (var type in await types.ListTypesAsync()) {
            Console.WriteLine($"Type: {type.Name}");
        }
    }
}
