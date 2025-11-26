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
- [x] Remove auth parameters from all cmdlets and rely solely on connection state:
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
- [x] PowerShell tests (Pester):
  - [x] Update existing tests to use `Connect-Sectigo` instead of per-cmdlet auth where applicable.
  - [x] Add tests for Admin-mode behaviour (including “not yet supported” paths until wired).
- [x] CI: ensure `dotnet test` and Pester suites pass locally and in CI for all target frameworks.

## 9. Async friendliness and PowerShell migration

The long-term execution order for remaining work is:

1. Complete API surface for the scenarios we support (covered in sections 1–5).
2. Extend tests for error paths and edge conditions (section 8).
3. Ensure the core library is fully async-friendly (no internal blocking on async calls).
4. Migrate PowerShell cmdlets to use `AsyncPSCmdlet` where appropriate.

Concretely:

- [x] Library async audit:
  - [x] Scan core library for `.Result` / `.GetAwaiter().GetResult()` and replace with async flow where feasible.
  - [x] Consider adding streaming helpers on `CertificateService` (for example, `EnumerateCertificatesAsync`) to avoid large in-memory lists.
- [x] PowerShell async migration:
  - [x] Convert certificate-centric cmdlets (`Get/Export/Remove/Update-SectigoCertificate`, `Get-SectigoCertificateKeystoreLink`, status/revocation) to derive from `AsyncPSCmdlet`.
  - [x] Gradually convert remaining cmdlets (`Get-SectigoOrders*`, `Wait-SectigoOrder`, etc.) where async semantics provide clear benefit.
  - [x] Keep synchronous behaviour for simple, quick operations where async does not materially improve UX.

## 10. Admin API – SSL extras (DCV, locations, metadata)

- [x] Add Admin SSL DCV and location support in C#:
  - [x] `GET /api/ssl/v2/{sslId}/dcv` – model and client method for current DCV status.
  - [x] `POST /api/ssl/v2/{sslId}/dcv/recheck` – client method to trigger DCV recheck.
  - [x] `GET /api/ssl/v2/{sslId}/location` – model and client method to list certificate locations.
  - [x] `GET /api/ssl/v2/{sslId}/location/{locationId}` – model and client method for a single location.
- [x] Add Admin SSL renewal/replace helpers:
  - [x] `POST /api/ssl/v2/renew/{renewId}` – client method for renew-by-renewId.
  - [x] `POST /api/ssl/v2/renew/manual/{id}` – client method for manual renew.
  - [x] `POST /api/ssl/v2/replace/{sslId}` – client method for replace-by-sslId.
- [x] Add Admin SSL metadata endpoints:
  - [x] `GET /api/ssl/v2/types` – Admin-specific certificate type listing.
  - [x] `GET /api/ssl/v2/customFields` – Admin-specific custom-field listing.
- [x] Decide on routing for these features:
  - [x] Extend `CertificateService` where concepts align (for example, types/custom fields).
  - [x] Keep DCV and locations on dedicated Admin-only client surface (C# only for now).
- [x] Add unit tests for all new Admin SSL operations (success and basic error paths).

## 11. Admin API – S/MIME certificates

- [x] Add a `SmimeAdminClient` (or equivalent) for `/api/smime/v2` endpoints:
  - [x] List & get: `GET /api/smime/v2`, `GET /api/smime/v2/{certId}`.
  - [x] Download/collect: `GET /api/smime/v2/collect/{backendCertId}`.
  - [x] Enroll/import: `POST /api/smime/v2/enroll` (import deferred).
  - [x] Keystore link: `POST /api/smime/v2/keystore/{certId}`.
  - [x] Renewal: `POST /api/smime/v2/renew/order/{backendCertId}`, `/renew/serial/{serialNumber}`.
  - [x] Revocation: `POST /api/smime/v2/revoke`, `/revoke/manual`, `/revoke/order/{backendCertId}`, `/revoke/serial/{serialNumber}`.
  - [x] Metadata: `GET /api/smime/v2/types`, locations: `GET /api/smime/v2/{certId}/location*`.
- [x] Introduce S/MIME models (requests/responses) aligned with `api.json`.
- [x] Decide on facade pattern:
  - [x] Introduce a dedicated `SmimeCertificateService` facade for S/MIME, keeping it separate from the SSL-centric `CertificateService` while reusing shared models where appropriate.
- [x] Add focused xUnit tests for S/MIME client (URI building, payloads, basic parsing).
- [ ] (Optional) Plan PowerShell/CLI exposure for S/MIME (separate phase).

## 12. Admin API – Device certificates

- [x] Add a `DeviceAdminClient` (or equivalent) for `/api/device/v1` endpoints:
  - [x] List & get device certificates: `GET /api/device/v1`, `GET /api/device/v1/{deviceCertId}`.
  - [x] Collect/download: `GET /api/device/v1/collect/{deviceCertId}`.
  - [x] Enroll: `POST /api/device/v1/enroll`.
  - [x] Renewal: `POST /api/device/v1/renew/order/{deviceCertId}`, `/renew/serial/{serialNumber}`.
  - [x] Import: `POST /api/device/v1/import` (JSON payload).
  - [x] Replace: `POST /api/device/v1/replace/order/{deviceCertId}`.
  - [x] Revocation: `POST /api/device/v1/revoke/order/{deviceCertId}`, `/revoke/serial/{serialNumber}`, `/revoke/manual`.
  - [x] Approve/decline: `POST /api/device/v1/approve/{deviceCertId}`, `/decline/{deviceCertId}`.
  - [x] Types and locations: `GET /api/device/v1/types`, `/location*`.
- [x] Introduce device models (requests/responses) and map core fields.
- [x] Add unit tests to verify URIs, payloads, and basic parsing.
- [ ] (Optional) Consider a shared abstraction with SSL/S/MIME for common operations (issue/renew/revoke/collect).

## 13. Admin API – Domains, organizations, DCV, and ACME

- [x] Domain management (`/api/domain/v1`):
  - [x] Client for listing/creating domains and updating state (activate/suspend/monitoring).
  - [x] Support delegation workflows: approve/reject, single and bulk delegation, and delegation listing via domain details.
- [ ] Organization validation (`/api/organization/v1` and `/organization/v2/.../validations`):
  - [x] Client to list organizations and managed-by reports (Admin organizations list, report-type and managed-by endpoints).
  - [-] Client for organization validations:
    - [x] List/get/delete validations and synchronize with CA backend (`/api/organization/v2/{orgId}/validations*`).
    - [ ] Submit/revalidate/validator assignment endpoints (modeled requests and helpers) – deferred.
- [ ] DCV operations (`/api/dcv/v2/*`):
  - [-] Client for starting DCV via CNAME/HTTP/HTTPS/TXT/email (start/submit flows).
  - [x] List and status queries, plus clear/delete endpoints, via a dedicated Admin DCV client.
- [ ] ACME accounts and servers:
  - [ ] Client for `/api/acme/v1/*`, `/api/acme/v2/*` (account/client/domain management).
  - [ ] Client for `/api/acme/v1/server` and `/api/acme/v1/evdetails/validation` as needed.
- [x] Global custom fields (`/api/customField/v2`):
  - [x] Client for listing custom fields across SSL/S/MIME/Device using the `certType` filter, plus get/create/update operations.
- [ ] Add tests for the above clients (URI building, minimal happy-path parsing) for domains, organizations, DCV, and ACME clients.

## 14. Admin API – Accounts, persons, endpoints, notifications, reports, templates

- [ ] Endpoint accounts (`/api/endpoint/v1/*`):
  - [ ] Client to list/create/update/delete endpoint accounts, configuration, and delegations.
- [ ] Persons (`/api/person/v2/*`):
  - [ ] Client to manage persons, invitations, and endpoint account associations.
- [ ] Notifications (`/api/notification/v1/*`):
  - [ ] Client to list/get notification definitions and types.
- [ ] Reports (`/api/report/v1/*`):
  - [ ] Client to fetch activity, SSL, device, domain reports (likely as streaming/download APIs).
- [x] Template administrators (`/api/admin-template/v1/*`):
  - [x] Client for listing, getting, creating, updating, and deleting IdP templates used for administrator SSO (`AdminTemplatesClient`).
  - [x] Tests to validate URI construction, query handling, and `Location` header parsing for create/update/delete operations.
- [ ] Tests for each new client in this group to validate URI construction and key behaviours.

## 15. Admin API – Admin accounts, connectors, discovery, Azure, and code signing

- [ ] Admin users and roles (`/api/admin/v1/*`):
  - [ ] Client to list/get admin users, manage passwords (`changepassword`, `password`), and unlink accounts.
  - [ ] Client to list roles, privileges, and IdP configurations (`/api/admin/v1/roles`, `/privileges`, `/idp`).
  - [ ] Tests to validate URI construction and basic parsing for admin account operations.
- [ ] Agents and network connectors (`/api/agent/v1/*`):
  - [ ] Client to manage MS and network agents, servers, and nodes (`/ms`, `/network`, `/server`, `/node`).
  - [ ] Tests to ensure correct URIs for agent, server, and node operations.
- [ ] Azure Key Vault accounts (`/api/azure/v1/*`):
  - [ ] Client to manage Azure accounts, checks, delegations, and to list resource groups and vaults.
  - [ ] Tests for URI construction and basic response parsing for Azure-related operations.
- [ ] DNS connectors (`/api/connector/v1/dns*`):
  - [ ] Client to manage DNS connector instances and providers.
  - [ ] Tests for connector create/update/delete and provider configuration endpoints.
- [ ] Code-signing certificates (`/api/cscert/v1/*`):
  - [ ] Client to support import and manual revocation of code-signing certificates.
  - [ ] Tests for request payloads and error handling for code-signing operations.
- [ ] Discovery tasks and buckets (`/api/discovery/v1/*`, `/api/discovery/v4/*`):
  - [ ] Client to manage discovery tasks (Azure/AD/network), including start/stop and operations (`/operation`, `/result`).
  - [ ] Client to manage discovery buckets, assignments, delegations, and rule execution.
  - [ ] Tests to validate URI construction and core task/bucket workflows.

## 15. Admin API – Discovery, agents, Azure, connectors, code-signing

- [ ] Discovery & agents:
  - [ ] Clients for `/api/discovery/v1/*` and `/api/discovery/v4/*` (tasks, operations, buckets, assignment rules).
  - [ ] Clients for `/api/agent/v1/ms*` and `/api/agent/v1/network*` (agent lifecycle, server/node operations).
- [ ] Azure accounts (`/api/azure/v1/*`):
  - [ ] Client for Azure account registration, validation, delegations, resource-group/vault enumeration.
- [ ] DNS connectors (`/api/connector/v1/dns*`):
  - [ ] Client for DNS connector registration and provider management.
- [ ] Code-signing certificates (`/api/cscert/v1/*`):
  - [ ] Client for code-signing import/revoke/manual operations.
- [ ] Admins and roles (`/api/admin/v1/*`):
  - [ ] Client for administrator users, roles, privileges, and password/IdP operations.
- [ ] Decide which of these higher-level admin/infra features should be in scope for this library vs. left to future iterations.
