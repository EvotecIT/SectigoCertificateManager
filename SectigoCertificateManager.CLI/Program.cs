using SectigoCertificateManager;
using SectigoCertificateManager.Clients;
using SectigoCertificateManager.Requests;
using System.Net.Http;

static void PrintUsage()
{
    Console.WriteLine("Commands:");
    Console.WriteLine("  get-ca-chain <certificateId> <outputPath>  Download issuing CA chain");
    Console.WriteLine("  search-orders [size] [position]            List orders page");
    Console.WriteLine("  renew-certificate <orderId> <csr> <dcvMode> [dcvEmail]  Renew certificate for order");
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
else if (string.Equals(args[0], "renew-certificate", StringComparison.OrdinalIgnoreCase))
{
    if (args.Length < 4 || !int.TryParse(args[1], out var orderId))
    {
        Console.WriteLine("Usage: renew-certificate <orderId> <csr> <dcvMode> [dcvEmail]");
        return;
    }

    var csr = args[2];
    var dcvMode = args[3];
    var dcvEmail = args.Length > 4 ? args[4] : null;

    var config = ApiConfigLoader.Load();
    using var httpClient = new HttpClient();
    var client = new SectigoClient(config, httpClient);
    var ordersClient = new OrdersClient(client);
    var request = new RenewCertificateRequest { Csr = csr, DcvMode = dcvMode, DcvEmail = dcvEmail };
    var newId = await ordersClient.RenewCertificateAsync(orderId, request);
    Console.WriteLine($"New certificate id: {newId}");
}
else
{
    PrintUsage();
}
