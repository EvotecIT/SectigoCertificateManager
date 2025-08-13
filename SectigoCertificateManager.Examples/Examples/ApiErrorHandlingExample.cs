using SectigoCertificateManager;
using System;
using System.Threading.Tasks;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates handling API error responses and enhanced messages.
/// </summary>
public static class ApiErrorHandlingExample {
    /// <summary>Runs the example showing enhanced error messages.</summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .Build();

        using var client = new SectigoClient(config);

        try {
            await client.GetAsync("v1/nonexistent");
        } catch (ApiException ex) {
            Console.WriteLine(ex.Message);
        }
    }
}
