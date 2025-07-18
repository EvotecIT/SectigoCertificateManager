using SectigoCertificateManager;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates accessing the underlying <see cref="System.Net.Http.HttpClient"/>.
/// </summary>
public static class HttpClientPropertyExample {
    /// <summary>Runs the example using the <see cref="SectigoClient.HttpClient"/> property.</summary>
    public static void Run() {
        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCredentials("<username>", "<password>")
            .WithCustomerUri("<customer uri>")
            .Build();

        using var client = new SectigoClient(config);

        // Adjust timeout for long-running operations.
        client.HttpClient.Timeout = TimeSpan.FromSeconds(30);
    }
}

