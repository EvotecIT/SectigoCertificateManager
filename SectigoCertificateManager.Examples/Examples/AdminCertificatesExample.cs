using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;

namespace SectigoCertificateManager.Examples.Examples;

/// <summary>
/// Demonstrates using the Admin Operations API with OAuth2 client credentials.
/// </summary>
/// <para>
/// Client credentials are generated in the Sectigo Certificate Manager portal
/// under the API Keys section. Use the configured client identifier and
/// client secret below.
/// </para>
public static class AdminCertificatesExample {
    /// <summary>
    /// Executes the example showing basic Admin API usage via <see cref="CertificateService"/>.
    /// </summary>
    public static async Task RunAsync() {
        var config = new AdminApiConfig(
            "https://admin.enterprise.sectigo.com",
            "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
            "<client id>",
            "<client secret>");

        using var service = new CertificateService(config);

        Console.WriteLine("Requesting latest certificates from Admin API...");
        var certificates = await service.ListAsync(size: 5, position: 0);
        foreach (var certificate in certificates) {
            Console.WriteLine($"{certificate.Id}: {certificate.CommonName}");
        }
    }
}

