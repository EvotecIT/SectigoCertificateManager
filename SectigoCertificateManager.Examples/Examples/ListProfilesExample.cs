using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates listing profiles using <see cref="ProfilesClient"/>.
/// </summary>
public static class ListProfilesExample {
    /// <summary>
    /// Executes the example that lists all profiles.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithApiVersion(ApiVersion.V25_6)
            .Build();

        var client = new SectigoClient(config);
        var profiles = new ProfilesClient(client);

        foreach (var profile in await profiles.ListProfilesAsync()) {
            Console.WriteLine($"Profile: {profile.Name}");
        }
    }
}
