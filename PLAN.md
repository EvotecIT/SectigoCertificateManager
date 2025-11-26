# SectigoCertificateManager – Admin API Integration Plan

> Status legend: `[ ]` not started, `[x]` done, `[-]` partially done

## 1. Core Admin API support (C#)

- [x] Add `AdminApiConfig` to hold Admin API + OAuth2 client-credentials settings.
- [x] Add `AdminSslIdentity` model mirroring `/api/ssl/v2` `SSLIdentity` schema.
- [x] Add `AdminSslClient` with:
  - [x] `ListAsync` → `GET /api/ssl/v2` (size/position)
  - [x] `GetAsync` → `GET /api/ssl/v2/{sslId}` (single certificate details)
  - [x] Additional SSL operations (renew, revoke, collect, enroll, import) mapped to `api.json`:
    - [x] Collect endpoint (`/collect/{sslId}`) for download.
    - [x] Renew by id endpoint (`/renewById/{sslId}`).
  - [x] Revoke endpoints:
      - [x] By id (`/revoke/{id}`) via `RevokeByIdAsync`.
      - [x] By serial/manual (`/revoke/serial/{serialNumber}`, `/revoke/manual`) via `RevokeBySerialAsync` and `MarkAsRevokedAsync`.
    - [x] Keystore / download:
      - [x] Keystore link (`/keystore/{sslId}/{formatType}`) via `CreateKeystoreLinkAsync` and `CertificateService.CreateKeystoreDownloadLinkAsync`.
      - [x] Download certificate (`/collect/{sslId}`) via `CollectAsync` and `CertificateService.DownloadCertificateAsync`.
    - [x] Enroll / import (`/enroll`, `/enroll-keygen`, `/import`)

## 2. Unified routing services (C#)

- [x] Introduce `CertificateService` in core library that routes between legacy and Admin clients:
  - [x] Constructor overload for `ApiConfig` (legacy) and `AdminApiConfig` (Admin).
  - [x] `Task<IReadOnlyList<Certificate>> ListAsync(...)`:
    - [x] Legacy → `CertificatesClient` search.
    - [x] Admin → `AdminSslClient.ListAsync` + projection to `Certificate`.
  - [x] `Task<Certificate?> GetAsync(int id, ...)`:
    - [x] Legacy → `CertificatesClient.GetAsync`.
    - [x] Admin → `AdminSslClient.GetAsync` + projection to `Certificate`.
  - [x] Additional operations routed through the service:
    - [x] Status and revocation (`GetStatusAsync`, `GetRevocationAsync`).
    - [x] Download (`DownloadCertificateAsync` → Admin collect / legacy download).
    - [x] Renewal (`RenewByIdAsync` → Admin renewById / legacy renew).
    - [x] Remove (`RemoveAsync` → Admin revokeById / legacy delete).
- [x] Add targeted unit tests for `CertificateService`:
  - [x] Legacy path: uses a fake `ISectigoClient`.
  - [x] Admin path: uses a fake `HttpMessageHandler` for `/api/ssl/v2`.

## 3. Connection model & configuration

- [x] Keep `ApiConfig` as the legacy SCM API configuration.
- [x] Add Admin configuration handling:
  - [x] `AdminApiConfig` type.
  - [x] `ConnectionHelper` in PowerShell layer to resolve active config.
- [x] `Connect-Sectigo` refactor:
  - [x] Legacy parameter set (`BaseUrl`, `Username`, `Password`, `CustomerUri`, `ApiVersion`):
    - [x] Store `ApiConfig` in `SectigoApiConfig` variable.
    - [x] Maintain `PSDefaultParameterValues` for backward compatibility.
  - [x] Admin parameter set (`ClientId`, `ClientSecret`, `Instance`, `AdminBaseUrl`, `TokenUrl`):
    - [x] Store `AdminApiConfig` in `SectigoAdminApiConfig` variable.
- [x] `Disconnect-Sectigo`:
  - [x] Clear legacy default parameter values.
  - [x] Remove `SectigoApiConfig` and `SectigoAdminApiConfig` variables.

## 4. PowerShell cmdlet surface unification

- [x] Unify certificate retrieval cmdlets:
  - [x] Replace dual `Get-SectigoCertificate` / `Get-SectigoCertificates` classes with a single cmdlet:
    - [x] Canonical name: `Get-SectigoCertificate` (singular).
    - [x] Alias: `Get-SectigoCertificates`.
    - [x] Parameter sets:
      - [x] `ById` set: `-CertificateId` (single certificate).
      - [x] `List` set: `-Size`, `-Position`, optional filters.
    - [x] Impl: use `CertificateService` to route Admin vs legacy.
- [-] Remove auth parameters from all cmdlets and rely solely on connection state:
  - [x] Certificates (list + single) – auth removed; use `CertificateService` and connection state.
  - [x] Inventory (`Get-SectigoInventory`) – legacy config only, rejects Admin for now.
  - [x] Certificate detail (`Get-SectigoCertificate`) – uses `CertificateService` for both Admin and legacy.
  - [x] Certificate status/revocation:
    - [x] `Get-SectigoCertificateStatus` – uses `CertificateService` for Admin and legacy.
    - [x] `Get-SectigoCertificateRevocation` – uses `CertificateService` for Admin and legacy.
  - [x] Export:
    - [x] `Export-SectigoCertificate` – uses `CertificateService` for Admin and legacy.
  - [x] Orders:
    - [x] `New-SectigoOrder`, `Invoke-SectigoCertificateRenewal`, `Update-SectigoCertificate` – legacy only, reject Admin.
    - [x] `Get-SectigoOrders`, `Get-SectigoOrdersPage` – legacy only, reject Admin.
  - [x] Organizations / profiles:
    - [x] `Get-SectigoOrganizations` – legacy only, rejects Admin.
    - [x] Profiles and related cmdlets – legacy only, use connection helper; Admin not yet supported.

## 5. Admin API support per operation (PowerShell)

### 5.1 Certificate list + detail

- [x] Admin list via `Get-SectigoCertificate -Size` (through `AdminSslClient.ListAsync`).
- [x] Admin single-cert via `Get-SectigoCertificate -CertificateId` (through `AdminSslClient.GetAsync`).
- [x] Tests:
  - [x] C#: Admin list & get calling correct `/api/ssl/v2` endpoints.
  - [x] PowerShell: covered indirectly via existing connection/validation tests; full end-to-end network tests are intentionally deferred.

### 5.2 Certificate status, revocation, export

- [x] Map Admin equivalents in `AdminSslClient` / related clients:
  - [x] Status retrieval via `AdminSslClient.GetAsync` + mapping in `CertificateService`.
  - [x] Revocation by id endpoint (`/revoke/{id}`) via `RevokeByIdAsync`.
  - [x] Additional revocation endpoints (`/revoke/serial`, `/revoke/manual`) via `RevokeBySerialAsync` and `MarkAsRevokedAsync`.
  - [x] Download/collect endpoint for export.
- [x] Update cmdlets:
  - [x] `Get-SectigoCertificateStatus` → uses `CertificateService` (Admin + legacy).
  - [x] `Get-SectigoCertificateRevocation` → uses `CertificateService` (Admin + legacy).
  - [x] `Export-SectigoCertificate` → uses `CertificateService.DownloadCertificateAsync` (Admin + legacy).
- [x] Tests for both modes:
  - [x] C#: `CertificateServiceTests` and `AdminSslClientTests` cover Admin status, revocation, download.
  - [x] PowerShell: Admin vs legacy behaviour validated for supported/unsupported cmdlets; Get-SectigoCertificate itself relies on C# coverage.

### 5.3 Inventory / search helpers

- [x] Legacy inventory (`Get-SectigoInventory`) uses `InventoryClient` and `inventory.csv`.
- [x] Admin equivalent:
  - [x] Inventory is not exposed separately in the Admin Operations API; Admin users are steered to `Get-SectigoCertificate -Size/-filters`.
- [x] Tests for selected approach.

### 5.4 Orders and organizations

- [x] Admin API mappings for orders:
  - [x] Renew/revoke operations are exposed via `CertificateService` (Admin SSL endpoints); there is no separate orders resource in the Admin Operations API.
  - [x] Enroll/import endpoints (`/api/ssl/v2/enroll`, `/enroll-keygen`, `/import`) are implemented on `AdminSslClient` for direct C# use.
- [x] Admin API mappings for organizations and profiles:
  - [x] Kept legacy-only for the initial Admin integration since the Admin API models validations and organizations differently; existing legacy clients remain available.
- [x] Cmdlets (`New-SectigoOrder`, `Get-SectigoOrders*`, `Get-SectigoOrganizations`, etc.) remain legacy-only by design for now and clearly reject Admin mode with a descriptive error.
- [x] Tests for both clients and cmdlets:
  - [x] C#: `AdminSslClientTests` and existing clients cover Admin operations; orders/orgs continue to rely on legacy tests.
  - [x] PowerShell: Pester tests assert legacy-only cmdlets throw clear errors when used with an Admin connection.

## 6. CLI alignment

- [x] Introduce a shared connection abstraction for CLI similar to PowerShell:
  - [x] Parse CLI options into either `ApiConfig` or Admin OAuth2 settings via environment variables.
  - [x] Use `CertificateService` for certificate-related commands (`get-ca-chain`).
- [x] Commands that currently call legacy endpoints should be updated to route via services and support Admin mode where appropriate:
  - [x] `get-ca-chain` supports both Admin and legacy mode via `CertificateService`.
  - [x] `search-orders` is intentionally kept legacy-only, since the Admin Operations API does not expose a general orders resource.
- [x] CLI tests:
  - [x] Covered indirectly via C# tests for `CertificateService` and clients; explicit CLI integration tests are deferred given the thin wrapper.

## 7. Documentation & examples

- [x] Update `README.md` PowerShell examples to:
  - [x] Use `Connect-Sectigo` (legacy) instead of per-cmdlet auth parameters.
  - [x] Add examples for Admin mode (`ClientId`/`ClientSecret`) with `Get-SectigoCertificate`.
- [x] Add a short “Choosing legacy vs Admin API” section.
- [x] Update examples in `SectigoCertificateManager.Examples` to include Admin API usage.

## 8. Testing and verification

- [x] C# tests:
  - [x] Add/extend xUnit tests for `AdminSslClient` and service routers.
  - [x] Ensure Admin paths are covered for list/get (and related service mapping). Legacy paths are already covered by existing tests.
- [-] PowerShell tests (Pester):
  - [x] Update existing tests to use `Connect-Sectigo` instead of per-cmdlet auth where applicable.
  - [x] Add tests for Admin-mode behaviour (including “not yet supported” paths until wired).
- [x] CI: ensure `dotnet test` and Pester suites pass locally and in CI for all target frameworks.
