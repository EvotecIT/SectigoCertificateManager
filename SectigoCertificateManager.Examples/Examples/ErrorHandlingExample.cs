using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates handling API errors.
/// </summary>
public static class ErrorHandlingExample {
    /// <summary>
    /// Executes the example showing how to catch <see cref="ApiException"/>.
    /// </summary>
    public static async Task RunAsync() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .Build();

        try {
            using var client = new SectigoClient(config);
            var certificates = new CertificatesClient(client);
            await certificates.GetAsync(-1);
        } catch (ApiException ex) {
            Console.WriteLine($"API error ({ex.ErrorCode}): {ex.Message}");
        }
    }
}