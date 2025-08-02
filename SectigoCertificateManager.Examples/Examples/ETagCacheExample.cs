using SectigoCertificateManager;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates enabling ETag caching for conditional requests.
/// </summary>
public static class ETagCacheExample {
    /// <summary>Runs the ETag cache example.</summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .WithETagCache()
            .Build();

        using var client = new SectigoClient(config);
        var first = await client.GetAsync("v1/resource").ConfigureAwait(false);
        var second = await client.GetAsync("v1/resource").ConfigureAwait(false);
    }
}
