using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates updating an organization using <see cref="OrganizationsClient"/>.
/// </summary>
public static class UpdateOrganizationExample {
    /// <summary>
    /// Executes the example that updates an organization.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var factory = new SectigoClientFactory();
        var client = factory.Create(config);
        var organizations = new OrganizationsClient(client);

        Console.WriteLine("Updating organization...");
        var request = new UpdateOrganizationRequest { Name = "New Name" };
        await organizations.UpdateAsync(123, request);
    }
}
