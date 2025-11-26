using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using SectigoCertificateManager.Utilities;
using System.Net.Http;

static void PrintUsage()
{
    Console.WriteLine("Commands:");
    Console.WriteLine("  get-ca-chain <certificateId> <outputPath>  Download issuing CA chain");
    Console.WriteLine("  search-orders [size] [position]            List orders page (legacy API only)");
}

static CertificateService CreateCertificateService()
{
    var clientId = Environment.GetEnvironmentVariable("SECTIGO_CLIENT_ID");
    var clientSecret = Environment.GetEnvironmentVariable("SECTIGO_CLIENT_SECRET");

    if (!string.IsNullOrWhiteSpace(clientId) && !string.IsNullOrWhiteSpace(clientSecret))
    {
        var adminBase = Environment.GetEnvironmentVariable("SECTIGO_ADMIN_BASE_URL")
                        ?? "https://admin.enterprise.sectigo.com";
        var tokenUrl = Environment.GetEnvironmentVariable("SECTIGO_TOKEN_URL")
                       ?? "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token";
        var trimmedBase = adminBase.TrimEnd('/');
        var adminConfig = new AdminApiConfig(trimmedBase, tokenUrl, clientId!, clientSecret!);
        return new CertificateService(adminConfig);
    }

    var legacyConfig = ApiConfigLoader.Load();
    return new CertificateService(legacyConfig);
}

if (args.Length == 0)
{
    PrintUsage();
    return;
}

if (string.Equals(args[0], "get-ca-chain", StringComparison.OrdinalIgnoreCase))
{
    if (args.Length < 3 || !int.TryParse(args[1], out var certId))
    {
        Console.WriteLine("Usage: get-ca-chain <certificateId> <outputPath>");
        return;
    }

    using var service = CreateCertificateService();
    using var certificate = await service.DownloadCertificateAsync(certId);
    CertificateExport.SavePemChain(certificate, args[2]);
    Console.WriteLine($"Chain written to {args[2]}");
}
else if (string.Equals(args[0], "search-orders", StringComparison.OrdinalIgnoreCase))
{
    var size = 200;
    var position = 0;
    if (args.Length > 1)
    {
        int.TryParse(args[1], out size);
    }
    if (args.Length > 2)
    {
        int.TryParse(args[2], out position);
    }

    var config = ApiConfigLoader.Load();
    using var httpClient = new HttpClient();
    var client = new SectigoClient(config, httpClient);
    var ordersClient = new OrdersClient(client);
    var request = new OrderSearchRequest { Size = size, Position = position };
    var result = await ordersClient.SearchAsync(request);
    if (result is not null)
    {
        foreach (var o in result.Orders)
        {
            Console.WriteLine($"Order ID: {o.Id}");
        }
    }
}
else
{
    PrintUsage();
}
