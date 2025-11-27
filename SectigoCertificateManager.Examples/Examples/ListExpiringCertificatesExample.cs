using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates listing certificates that expire within a specified window using the Admin Operations API.
/// </summary>
public static class ListExpiringCertificatesExample {
    /// <summary>
    /// Executes the example showing how to use <see cref="CertificateService.ListExpiringAsync"/>.
    /// </summary>
    public static async Task RunAsync() {
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "<client id>",
            "<client secret>");

        using var service = new CertificateService(config);

        var days = 30;
        Console.WriteLine($"Listing Issued certificates that expire within the next {days} days...");

        var certificates = await service.ListExpiringAsync(
            days,
            CertificateStatus.Issued,
            null,
            null,
            CancellationToken.None);

        foreach (var certificate in certificates) {
            Console.WriteLine($"{certificate.Id}: {certificate.CommonName} (Expires: {certificate.Expires})");
        }
    }
}
