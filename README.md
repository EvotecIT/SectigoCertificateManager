# SectigoCertificateManager



This library provides a simple client for the Sectigo Certificate Manager API.



## Fluent API



Create an `ApiConfig` using the fluent builder:



```csharp

var config = new ApiConfigBuilder()

    .WithBaseUrl("https://example.com")

    .WithCredentials("user", "pass")

    .WithCustomerUri("cst1")

    .WithApiVersion(ApiVersion.V25_5)

    .Build();

```



Use the resulting `ApiConfig` to instantiate `SectigoClient`.


## PowerShell Module

Import the module and call the cmdlets:

```powershell
Import-Module ./SectigoCertificateManager.PowerShell.dll

Get-SectigoCertificate -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -CertificateId 123

New-SectigoOrder -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -CommonName "example.com" -ProfileId 1
```
