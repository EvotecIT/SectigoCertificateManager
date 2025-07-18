using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates listing IdP templates using <see cref="AdminTemplatesClient"/>.
/// </summary>
public static class ListIdpTemplatesExample {
    /// <summary>Executes the example.</summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var templates = new AdminTemplatesClient(client);

        foreach (var template in await templates.ListAsync()) {
            Console.WriteLine($"Template: {template.Name}");
        }
    }
}
