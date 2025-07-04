# SectigoCertificateManager



This library provides a simple client for the Sectigo Certificate Manager API.

The library defaults to **API version 25.4** as defined in `ApiConfigBuilder`.
Support for version 25.5 is also available via `ApiVersion.V25_5`.

## Documentation

HTML copies of the official API reference are included in the repository:

- [certmgr-api-doc-25.4.html](Documentation/certmgr-api-doc-25.4.html)
- [certmgr-api-doc-25.5.html](Documentation/certmgr-api-doc-25.5.html)




## Fluent API



Create an `ApiConfig` using the fluent builder:



```csharp

var config = new ApiConfigBuilder()

    .WithBaseUrl("https://example.com")

    .WithCredentials("user", "pass")

    .WithCustomerUri("cst1")

    .WithApiVersion(ApiVersion.V25_5)

    // configure handler or attach a client certificate if needed
    .WithHttpClientHandler(h => h.AllowAutoRedirect = false)
    .WithClientCertificate(myCert)

    .Build();

```



Use the resulting `ApiConfig` to instantiate `SectigoClient`.

```csharp
var client = new SectigoClient(config);
var revocations = new RevocationsClient(client);
await revocations.RevokeAsync(123, "Key compromised");
```


## PowerShell Module

Import the module and call the cmdlets:

```powershell
Import-Module ./SectigoCertificateManager.PowerShell.dll

Get-SectigoCertificate -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -CertificateId 123

New-SectigoOrder -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -CommonName "example.com" -ProfileId 1
```
