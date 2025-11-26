# SectigoCertificateManager – Admin API Integration Plan

> Status legend: `[ ]` not started, `[x]` done, `[-]` partially done

## 1. Core Admin API support (C#)

- [x] Add `AdminApiConfig` to hold Admin API + OAuth2 client-credentials settings.
- [x] Add `AdminSslIdentity` model mirroring `/api/ssl/v2` `SSLIdentity` schema.
- [x] Add `AdminSslClient` with:
  - [x] `ListAsync` → `GET /api/ssl/v2` (size/position)
  - [ ] `GetAsync` → `GET /api/ssl/v2/{sslId}` (single certificate details)
  - [ ] Additional SSL operations (renew, revoke, collect, etc.) mapped to `api.json`:
    - [ ] Renew endpoints (`/renewById`, `/renew`, `/renew/manual`)
    - [ ] Revoke endpoints (`/revoke/{id}`, `/revoke/serial/{serialNumber}`, `/revoke/manual`)
    - [ ] Keystore / download (`/keystore/{sslId}/{formatType}`, `/collect/{sslId}`)
    - [ ] Enroll / import (`/enroll`, `/enroll-keygen`, `/import`)

## 2. Unified routing services (C#)

- [ ] Introduce `CertificateService` in core library that routes between legacy and Admin clients:
  - [ ] Constructor overload for `ApiConfig` (legacy) and `AdminApiConfig` (Admin).
  - [ ] `Task<IReadOnlyList<Certificate>> ListAsync(...)`:
    - [ ] Legacy → `CertificatesClient` search.
    - [ ] Admin → `AdminSslClient.ListAsync` + projection to `Certificate`.
  - [ ] `Task<Certificate?> GetAsync(int id, ...)`:
    - [ ] Legacy → `CertificatesClient.GetAsync`.
    - [ ] Admin → `AdminSslClient.GetAsync` + projection to `Certificate`.
  - [ ] Later: `RevokeAsync`, `RenewAsync`, `DownloadAsync`, etc., routed similarly.
- [ ] Add targeted unit tests for `CertificateService`:
  - [ ] Legacy path: uses a fake `ISectigoClient` or WireMock endpoints.
  - [ ] Admin path: uses a fake `HttpMessageHandler` or WireMock endpoints for `/api/ssl/v2`.

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

- [ ] Unify certificate retrieval cmdlets:
  - [ ] Replace dual `Get-SectigoCertificate` / `Get-SectigoCertificates` classes with a single cmdlet:
    - [ ] Canonical name: `Get-SectigoCertificate` (singular).
    - [ ] Alias: `Get-SectigoCertificates`.
    - [ ] Parameter sets:
      - [ ] `ById` set: `-CertificateId` (single certificate).
      - [ ] `List` set: `-Size`, `-Position`, optional filters.
    - [ ] Impl: use `CertificateService` to route Admin vs legacy.
- [ ] Remove auth parameters from all cmdlets and rely solely on connection state:
  - [x] Certificates (list + single) – partially done, still needs consolidation.
  - [x] Inventory (`Get-SectigoInventory`) – now uses legacy config only, rejects Admin for now.
  - [x] Certificate detail (`Get-SectigoCertificate`) – uses legacy config only, rejects Admin for now.
  - [x] Certificate status/revocation:
    - [x] `Get-SectigoCertificateStatus` – legacy only, rejects Admin.
    - [x] `Get-SectigoCertificateRevocation` – legacy only, rejects Admin.
  - [x] Export:
    - [x] `Export-SectigoCertificate` – legacy only, rejects Admin.
  - [x] Orders:
    - [x] `New-SectigoOrder`, `Renew-SectigoCertificate`, `Update-SectigoCertificate` – legacy only, reject Admin.
    - [x] `Get-SectigoOrders`, `Get-SectigoOrdersPage` – legacy only, reject Admin (requires refactor).
  - [x] Organizations / profiles:
    - [x] `Get-SectigoOrganizations` – legacy only, rejects Admin.
    - [ ] Profiles and related cmdlets – still to be refactored to use connection helper.

## 5. Admin API support per operation (PowerShell)

For each area below, the plan is:
- Wire Admin-side operations in core (`AdminSslClient` + additional Admin clients if needed).
- Route via `CertificateService` (or similar service) where appropriate.
- Update cmdlets to support Admin mode rather than rejecting it.
- Add/extend tests (xUnit + Pester) to cover both modes.

### 5.1 Certificate list + detail

- [x] Admin list via `Get-SectigoCertificate -Size` (through `AdminSslClient.ListAsync`).
- [ ] Admin single-cert via `Get-SectigoCertificate -CertificateId` (through `AdminSslClient.GetAsync`).
- [ ] Tests:
  - [ ] C#: Admin list & get calling correct `/api/ssl/v2` endpoints.
  - [ ] Pester: `Get-SectigoCertificate` behaves correctly in Admin vs legacy mode.

### 5.2 Certificate status, revocation, export

- [ ] Map Admin equivalents in `AdminSslClient` / related clients:
  - [ ] Status endpoint (if available in Admin API).
  - [ ] Revocation endpoints (`/revoke`, `/revoke/serial`, `/revoke/manual`).
  - [ ] Download/collect/keystore endpoints for export.
- [ ] Update cmdlets:
  - [ ] `Get-SectigoCertificateStatus`
  - [ ] `Get-SectigoCertificateRevocation`
  - [ ] `Export-SectigoCertificate`
  - All to use services that route Admin vs legacy instead of rejecting Admin.
- [ ] Tests for both modes (C# + Pester).

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

- [ ] Update `README.md` PowerShell examples to:
  - [ ] Use `Connect-Sectigo` (legacy) instead of per-cmdlet auth parameters.
  - [ ] Add examples for Admin mode (`ClientId`/`ClientSecret`) with `Get-SectigoCertificate`.
- [ ] Add a short “Choosing legacy vs Admin API” section.
- [ ] Update examples in `SectigoCertificateManager.Examples` to include Admin API usage.

## 8. Testing and verification

- [ ] C# tests:
  - [ ] Add/extend xUnit tests for `AdminSslClient` and service routers.
  - [ ] Ensure both Admin and legacy paths are covered for list/get at minimum.
- [ ] PowerShell tests (Pester):
  - [ ] Update existing tests to use `Connect-Sectigo` instead of per-cmdlet auth where applicable.
  - [ ] Add tests for Admin-mode behaviour (including “not yet supported” paths until wired).
- [ ] CI: ensure `dotnet test` and Pester suites pass locally and in CI for all target frameworks.

