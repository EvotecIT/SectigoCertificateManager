using SectigoCertificateManager;
using System;
using System.IO;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates storing and loading authentication tokens from a cache file.
/// </summary>
public static class TokenCacheExample {
    /// <summary>Runs the example.</summary>
    public static void Run() {
        var path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".sectigo", "token.json");
        var info = new TokenInfo("<token>", DateTimeOffset.UtcNow.AddHours(1));
        ApiConfigLoader.WriteToken(info, path);

        var cached = ApiConfigLoader.ReadToken(path);
        if (cached is null) {
            Console.WriteLine("No cached token available.");
            return;
        }

        var config = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCustomerUri("<customer uri>")
            .WithToken(cached.Token)
            .WithTokenExpiration(cached.ExpiresAt)
            .Build();

        using var client = new SectigoClient(config);
        Console.WriteLine($"Loaded token expires at {cached.ExpiresAt:u}");
    }
}
