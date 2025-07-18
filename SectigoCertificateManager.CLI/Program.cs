using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using System.Net.Http;

static void PrintUsage()
{
    Console.WriteLine("Commands:");
    Console.WriteLine("  get-ca-chain <certificateId> <outputPath>  Download issuing CA chain");
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

    var config = ApiConfigLoader.Load();
    using var httpClient = new HttpClient();
    var client = new SectigoClient(config, httpClient);
    var certificates = new CertificatesClient(client);

    await certificates.GetCaChainAsync(certId, args[2]);
    Console.WriteLine($"Chain written to {args[2]}");
}
else
{
    PrintUsage();
}
