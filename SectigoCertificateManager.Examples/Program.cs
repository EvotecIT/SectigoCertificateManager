using SectigoCertificateManager.Examples.Examples;

await BasicApiExample.RunAsync();
await SearchCertificatesExample.RunAsync();
await DownloadCertificateExample.RunAsync();
await DeleteCertificateExample.RunAsync();
await UploadOrdersExample.RunAsync();
await GetCertificateRevocationExample.RunAsync();
await ImportCertificatesExample.RunAsync();
await ListCertificateTypesExample.RunAsync();
await WatchOrdersExample.RunAsync();
CsrGeneratorExample.Run();
