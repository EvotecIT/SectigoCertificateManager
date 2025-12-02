# SectigoCertificateManager

This library provides a simple client for the Sectigo Certificate Manager API.

The library defaults to **API version 25.6** as defined in `ApiConfigBuilder`.
Support for version 25.5 remains available via `ApiVersion.V25_5`. To target
version 25.6 explicitly, use `ApiVersion.V25_6`.

## Installation

- NuGet: `dotnet add package SectigoCertificateManager`
- PowerShell module (built from this repo): `Import-Module SectigoCertificateManager`
 - Targets: `net10.0`, `net9.0`, `net8.0`, `netstandard2.0`, `net472`
- License: MIT · Source: https://github.com/EvotecIT/SectigoCertificateManager

The core library now supports two connection modes:

- **Legacy SCM API** - username/password + customer URI (`ApiConfig`).
- **Admin Operations API** - OAuth2 client credentials (`AdminApiConfig`) with
  routing handled by `CertificateService`.

## Choosing legacy vs Admin API

- Use the **legacy SCM API** when you already rely on username/password
  credentials and require features that are not yet exposed via the Admin
  Operations API (for example, some inventory and order/organization flows).
- Use the **Admin Operations API** when you want modern OAuth2 client
  credentials, better alignment with the web portal’s "Admin" experience, and
  access to newer SSL endpoints such as `/api/ssl/v2`.

## Documentation

HTML copies of the official API reference are included in the repository:

- [certmgr-api-doc-25.4.html](Documentation/certmgr-api-doc-25.4.html)
- [certmgr-api-doc-25.5.html](Documentation/certmgr-api-doc-25.5.html)
- [Admin API OpenAPI spec](Documentation/api.json) - this file is a snapshot of the Admin Operations
  API OpenAPI document downloaded from the Sectigo portal. To refresh it, download the latest JSON
  from the Admin API docs URL and replace this file.

## Fluent API (legacy SCM)

Create an `ApiConfig` using the fluent builder:

```csharp
var config = new ApiConfigBuilder()
    .WithBaseUrl("https://cert-manager.com/api")
    .WithCredentials("user", "pass")
    .WithCustomerUri("cst1")
    .WithApiVersion(ApiVersion.V25_6)
    .WithConcurrencyLimit(5)
    // configure handler or attach a client certificate if needed
    .WithHttpClientHandler(h => h.AllowAutoRedirect = false)
    .WithClientCertificate(myCert)
    .Build();

using var client = new SectigoClient(config);
var certificates = new CertificatesClient(client);
var cert = await certificates.GetAsync(12345);
```

## Fluent API (Admin Operations API + CertificateService)

Use OAuth2 client credentials generated in the **API Keys** area of the
Sectigo Certificate Manager portal, and route calls through
`CertificateService`:

```csharp
using SectigoCertificateManager;
using SectigoCertificateManager.AdminApi;

var adminConfig = new AdminApiConfig(
    "https://admin.enterprise.sectigo.com",
    "https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token",
    "<client id>",
    "<client secret>");

using var service = new CertificateService(adminConfig);
var list = await service.ListAsync(size: 10, position: 0);

foreach (var cert in list)
{
    Console.WriteLine($"{cert.Id}: {cert.CommonName}");
}
```

The same `CertificateService` can be constructed from `ApiConfig` to talk to
the legacy API; callers do not need to care which API is active.

## PowerShell Module

Import the module once, then connect using either legacy or Admin mode.
Subsequent cmdlets reuse the active connection.

```powershell
Import-Module ./SectigoCertificateManager.PowerShell.dll
```

### Legacy connection (username/password)

```powershell
Connect-Sectigo -BaseUrl "https://cert-manager.com/api" `
                -Username "user" `
                -Password "pass" `
                -CustomerUri "tenant1" `
                -ApiVersion V25_6

# Retrieve a single certificate
Get-SectigoCertificate -CertificateId 12345

# List certificates
Get-SectigoCertificate -Size 50 -Position 0

# Download a certificate
Export-SectigoCertificate -CertificateId 12345 -Path './cert.pem'

# Check status / revocation
Get-SectigoCertificateStatus -CertificateId 12345
Get-SectigoCertificateRevocation -CertificateId 12345

# Legacy-only operations (inventory, orders, organizations):
Get-SectigoInventory
Get-SectigoOrders
Get-SectigoOrganizations
```

### Admin Operations API connection (OAuth2 client credentials)

```powershell
Connect-Sectigo -ClientId "<client id>" `
                -ClientSecret "<client secret>" `
                -Instance "enterprise" `
                -AdminBaseUrl "https://admin.enterprise.sectigo.com"

# The same cmdlets route through the Admin API:
Get-SectigoCertificate -CertificateId 17331734
Export-SectigoCertificate -CertificateId 17331734 -Path './admin-cert.pem'
Export-SectigoCertificate -CertificateId 17331734 -Format Pfx -Path './admin-cert.pfx' -PfxPassword (Read-Host -AsSecureString "Pfx password")
Get-SectigoCertificateStatus -CertificateId 17331734
Get-SectigoCertificateRevocation -CertificateId 17331734

# List latest certificates (Admin summary vs. detailed)
Get-SectigoCertificate -Size 30
Get-SectigoCertificate -Size 30 -Detailed

# Filter by status / requester / expiration (Admin only)
Get-SectigoCertificate -Size 50 -Status Issued -Requester 'user@example.com'
Get-SectigoCertificate -Size 50 -ExpiresBefore (Get-Date).AddDays(30)
Get-SectigoCertificate -Status Issued -ExpiresWithinDays 30

# Renew (Admin or legacy) and revoke with typed enums
# - Admin: use -CertificateId with an Admin connection
# - Legacy: use -OrderNumber with a legacy connection
Invoke-SectigoCertificateRenewal -CertificateId 17331734 -Csr (Get-Content .\new.csr -Raw) -DcvMode Email -DcvEmail 'admin@example.com'
# Legacy path:
# Invoke-SectigoCertificateRenewal -OrderNumber 10 -Csr 'CSR' -DcvMode Email -DcvEmail 'admin@example.com'

# Notes on renewals
# - The Admin Operations API requires a CSR for renewals (Sectigo does not auto-generate keys for you).
# - If you need a CSR at runtime, use the CsrGenerator helper (see SectigoCertificateManager.Examples) before calling Invoke-SectigoCertificateRenewal.
# - After renewal, download the new certificate for delivery:
#     Export-SectigoCertificate -CertificateId $newId -Path './renewed.cer'
#     Export-SectigoCertificate -CertificateId $newId -Format Pfx -PfxPassword (Read-Host -AsSecureString 'Password') -Path './renewed.pfx'

# Generate a CSR (PowerShell)
$csr = New-SectigoCsr -CommonName 'example.com' -DnsName 'example.com','www.example.com' -Organization 'Example' -Country 'US'

# Use generated CSR for Admin renew
Invoke-SectigoCertificateRenewal -CertificateId 11552108 -Csr $csr.Csr -DcvMode Email -DcvEmail 'admin@example.com'

# Use generated CSR for a legacy order
$order = New-SectigoOrder -CertificateType 501 -Term 365 -Csr $csr.Csr -SubjectAlternativeNames 'example.com','www.example.com'

Remove-SectigoCertificate -CertificateId 17331734 -ReasonCode KeyCompromise -Reason 'Key compromised'

# Inventory and most order/organization-related cmdlets currently remain
# legacy-only and will throw if used with an Admin connection.
```

Use `-SubjectAlternativeNames` on `New-SectigoOrder` to specify multiple SAN
values when placing an order (legacy mode only for now).

## CLI

The CLI shares the same routing logic as PowerShell: if Admin OAuth2
environment variables are present it uses the Admin API; otherwise it uses the
legacy configuration loaded by `ApiConfigLoader`.

### Legacy usage

Configure your legacy API settings in the JSON file consumed by
`ApiConfigLoader` (see `ApiConfigLoaderTests` for examples), then run:

```bash
dotnet run --project SectigoCertificateManager.CLI get-ca-chain 123 ./chain.pem
```

### Admin Operations API usage

```bash
export SECTIGO_CLIENT_ID="<client id>"
export SECTIGO_CLIENT_SECRET="<client secret>"
export SECTIGO_ADMIN_BASE_URL="https://admin.enterprise.sectigo.com"
export SECTIGO_TOKEN_URL="https://auth.sso.sectigo.com/auth/realms/apiclients/protocol/openid-connect/token"

dotnet run --project SectigoCertificateManager.CLI get-ca-chain 17331734 ./chain.pem

# List certificates expiring in the next 30 days (Admin only, using CertificateStatus enum)
dotnet run --project SectigoCertificateManager.CLI list-expiring 30 Issued
```

The `search-orders` CLI command currently remains legacy-only and uses the
classic SCM API endpoints.
