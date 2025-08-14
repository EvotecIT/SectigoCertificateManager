using SectigoCertificateManager;
using SectigoCertificateManager.Clients;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates downloading an issued certificate as a password-protected PFX.
/// </summary>
public static class DownloadPfxExample {
    /// <summary>
    /// Executes the example that downloads a PFX containing the private key.
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

        Console.WriteLine("Downloading PFX certificate...");
        using var cert = await certificates.DownloadPfxAsync(12345, "secret");
        Console.WriteLine($"Thumbprint: {cert.Thumbprint}");
    }
}
