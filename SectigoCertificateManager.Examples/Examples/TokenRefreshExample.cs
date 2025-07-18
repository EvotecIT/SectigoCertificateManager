using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Net.Http.Json;
using System.Text.Json;
using SectigoCertificateManager.Utilities;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates automatically refreshing the authentication token.
/// </summary>
public static class TokenRefreshExample {
    private static readonly JsonSerializerOptions s_json = new() {
        PropertyNameCaseInsensitive = true
    };
    /// <summary>Runs the example using <see cref="ApiConfigBuilder.WithTokenRefresh"/>.</summary>
    public static async Task RunAsync() {
        var builder = new ApiConfigBuilder()
            .WithBaseUrl("https://cert-manager.com/api")
            .WithCustomerUri("<customer uri>")
            .WithToken("<initial token>")
            .WithTokenExpiration(DateTimeOffset.UtcNow.AddMinutes(30))
            .WithTokenRefresh(RefreshTokenAsync);

        var config = builder.Build();

        var client = new SectigoClient(config);
        var certificates = new CertificatesClient(client);

        Console.WriteLine("Requesting certificate details using refreshed token...");
        var certificate = await certificates.GetAsync(12345);
        Console.WriteLine($"Common name: {certificate?.CommonName}");
    }

    private static async Task<TokenInfo> RefreshTokenAsync(CancellationToken cancellationToken) {
        using var http = new HttpClient();
        var request = new HttpRequestMessage(HttpMethod.Post, "https://cert-manager.com/api/v1/auth");
        request.Headers.Add("login", "<username>");
        request.Headers.Add("password", "<password>");
        request.Headers.Add("customerUri", "<customer uri>");

        var response = await http.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        var data = await response.Content
            .ReadFromJsonAsyncSafe<LoginResponse>(s_json, cancellationToken)
            .ConfigureAwait(false);
        var expires = DateTimeOffset.UtcNow.AddSeconds(data!.Expires);
        return new TokenInfo(data.Token, expires);
    }

    private sealed record LoginResponse(string Token, int Expires);
}
