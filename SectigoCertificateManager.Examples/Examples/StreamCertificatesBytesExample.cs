using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.IO;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates streaming certificates as byte arrays and writing them to disk.
/// </summary>
public static class StreamCertificatesBytesExample {
    /// <summary>
    /// Executes the example that streams certificate bytes.
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

        var index = 0;
        await foreach (var bytes in certificates.StreamCertificatesAsync<byte[]>(pageSize: 50)) {
            var path = Path.Combine("certs", $"certificate{index++}.cer");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            await File.WriteAllBytesAsync(path, bytes);
        }
    }
}
