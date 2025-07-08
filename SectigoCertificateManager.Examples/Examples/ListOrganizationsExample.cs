using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates listing organizations using <see cref="OrganizationsClient"/>.
/// </summary>
public static class ListOrganizationsExample {
    /// <summary>
    /// Executes the example that lists all organizations.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var organizations = new OrganizationsClient(client);

        foreach (var org in await organizations.ListOrganizationsAsync()) {
            Console.WriteLine($"Organization: {org.Name}");
        }
    }
}
