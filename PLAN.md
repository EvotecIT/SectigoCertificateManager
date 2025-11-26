# SectigoCertificateManager – Admin API Integration Plan

> Status legend: `[ ]` not started, `[x]` done, `[-]` partially done

## 1. Core Admin API support (C#)

- [x] Add `AdminApiConfig` to hold Admin API + OAuth2 client-credentials settings.
- [x] Add `AdminSslIdentity` model mirroring `/api/ssl/v2` `SSLIdentity` schema.
- [x] Add `AdminSslClient` with:
  - [x] `ListAsync` → `GET /api/ssl/v2` (size/position)
  - [x] `GetAsync` → `GET /api/ssl/v2/{sslId}` (single certificate details)
  - [-] Additional SSL operations (renew, revoke, collect, etc.) mapped to `api.json`:
    - [x] Collect endpoint (`/collect/{sslId}`) for download.
    - [x] Renew by id endpoint (`/renewById/{sslId}`).
  - [x] Revoke endpoints:
      - [x] By id (`/revoke/{id}`) via `RevokeByIdAsync`.
      - [x] By serial/manual (`/revoke/serial/{serialNumber}`, `/revoke/manual`) via `RevokeBySerialAsync` and `MarkAsRevokedAsync`.
    - [-] Keystore / download:
      - [x] Keystore link (`/keystore/{sslId}/{formatType}`) via `CreateKeystoreLinkAsync` and `CertificateService.CreateKeystoreDownloadLinkAsync`.
      - [x] Download certificate (`/collect/{sslId}`) via `CollectAsync` and `CertificateService.DownloadCertificateAsync`.
    - [ ] Enroll / import (`/enroll`, `/enroll-keygen`, `/import`)

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
- [-] Add targeted unit tests for `CertificateService`:
  - [ ] Legacy path: uses a fake `ISectigoClient` or WireMock endpoints (covered today by broader client tests, but not a dedicated service test).
  - [x] Admin path: uses a fake `HttpMessageHandler` or WireMock endpoints for `/api/ssl/v2`.

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

For each area below, the plan is:
- Wire Admin-side operations in core (`AdminSslClient` + additional Admin clients if needed).
- Route via `CertificateService` (or similar service) where appropriate.
- Update cmdlets to support Admin mode rather than rejecting it.
- Add/extend tests (xUnit + Pester) to cover both modes.

### 5.1 Certificate list + detail

- [x] Admin list via `Get-SectigoCertificate -Size` (through `AdminSslClient.ListAsync`).
- [x] Admin single-cert via `Get-SectigoCertificate -CertificateId` (through `AdminSslClient.GetAsync`).
- [ ] Tests:
  - [x] C#: Admin list & get calling correct `/api/ssl/v2` endpoints.
  - [ ] Pester: `Get-SectigoCertificate` behaves correctly in Admin vs legacy mode.

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
- [-] Tests for both modes:
  - [x] C#: `CertificateServiceTests` and `AdminSslClientTests` cover Admin status, revocation, download.
  - [ ] Pester: cmdlets in Admin vs legacy mode (no network) still to be expanded.

### 5.3 Inventory / search helpers

- [x] Legacy inventory (`Get-SectigoInventory`) uses `InventoryClient` and `inventory.csv`.
- [ ] Decide Admin equivalent:
  - Either:
    - [ ] Implement an Admin-side “inventory” projection using `/api/ssl/v2` plus filters.
  - Or:
    - [ ] Deprecate inventory for Admin mode and steer users to `Get-SectigoCertificate -Size/-filters`.
- [ ] Tests for selected approach.

### 5.4 Orders and organizations

- [ ] Admin API mappings for orders: enroll, renew, revoke, import.
- [ ] Admin API mappings for organizations and profiles (if exposed by Admin API spec).
- [ ] Update cmdlets (`New-SectigoOrder`, `Get-SectigoOrders*`, `Get-SectigoOrganizations`, etc.) to:
  - [ ] Use a routing service similar to `CertificateService` for orders/orgs.
  - [ ] Support both Admin and legacy mode where meaningful.
- [ ] Tests for both clients and cmdlets.

## 6. CLI alignment

- [ ] Introduce a shared connection abstraction for CLI similar to PowerShell:
  - [ ] Parse CLI options into either `ApiConfig` or `AdminApiConfig`.
  - [ ] Use `CertificateService` (and later other services) instead of talking directly to low-level clients.
- [ ] Commands that currently call legacy endpoints should be updated to route via services and support Admin mode where appropriate.
- [ ] Add/extend CLI tests to cover both Admin and legacy modes.

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
  - [ ] Add tests for Admin-mode behaviour (including “not yet supported” paths until wired).
- [ ] CI: ensure `dotnet test` and Pester suites pass locally and in CI for all target frameworks.
