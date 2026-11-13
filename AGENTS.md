# SectigoCertificateManager

C# client library, PowerShell module and CLI for managing Sectigo certificates
through both the legacy SCM REST API (username/password) and the Admin
Operations API (OAuth2 client credentials).

## Layout

- `SectigoCertificateManager/`: the library. `SectigoClient` and `Clients/` for
  the legacy API, `AdminApi/` for the Admin Operations API, `CertificateService`
  routing across both, plus `Models/`, `Requests/`, `Responses/`, `Utilities/`.
- `SectigoCertificateManager.PowerShell/` and `Module/`: binary cmdlets, module
  manifest, build (`Module/Build/Build-Module.ps1`) and Pester tests (`Module/Tests/`).
- `SectigoCertificateManager.CLI/`: command-line tool.
- `SectigoCertificateManager.Tests/`: xUnit tests.
  `SectigoCertificateManager.Examples/` and `.Benchmarks/`: samples and benchmarks.
- `Documentation/`: vendor API references. `api.json` is a snapshot of the Admin
  API OpenAPI spec; the HTML files are the legacy API docs.
- `PLAN.md`: Admin API coverage checklist. Update it when you add an endpoint.
- `Build/Build-Project.ps1` with `Build/project.build.json`: NuGet packaging.

## Build and test

```powershell
dotnet build SectigoCertificateManager.sln -c Release
dotnet test SectigoCertificateManager.sln -c Release
dotnet test SectigoCertificateManager.Tests/SectigoCertificateManager.Tests.csproj -f net8.0 --filter "FullyQualifiedName~ClassName"
./Module/Build/Build-Module.ps1 -SignModule $false                               # build the module
./Module/Build/Build-Module.ps1 -ConfigurationGateMode Documentation -SignModule $false   # regenerate Module/Docs
./Module/SectigoCertificateManager.Tests.ps1                                     # module Pester tests
./Build/Build-Project.ps1                                                        # NuGet packages into Artefacts/ProjectBuild
```

- Library targets: `net472`, `netstandard2.0`, `net8.0`, `net9.0`, `net10.0`.
  PowerShell: `net472` and `net8.0`. Tests: `net472`, `net8.0`, `net9.0`
  (`net472` needs Mono outside Windows).
- `-ConfigurationGateMode Publish`, `Build-Project.ps1 -PublishNuget $true` and
  `-PublishGitHub $true` are release-only.
- Nullable is enabled and warnings are errors (`Directory.Build.props`).
- The source module loads cmdlets from
  `SectigoCertificateManager.PowerShell/bin/Debug`, so run a Debug
  `dotnet build` before the module Pester tests.

## Generated files

`Module/Docs/*.md` and `Module/en-US/SectigoCertificateManager-help.xml` come
from the cmdlet XML documentation. Regenerate with the Documentation mode;
don't edit them by hand.

## Conventions

- C#: K&R braces, braces on every block, file-scoped namespaces, follow
  `.editorconfig`. Split large classes with `partial` (as `CertificateService`).
- Cmdlet docs use XML documentation that PowerForge turns into Markdown and
  external help: `<summary>` for the synopsis, `<para>` for further description
  paragraphs, `<example>` with `<prefix>`, `<code>` and `<para>`.
- Cmdlets work in both connection modes where the API allows it. Legacy-only
  and Admin-only operations must fail clearly in the other mode.
- PowerShell scripts: Windows PowerShell 5.1 and 7 compatible, K&R/OTBS braces,
  full parameter names, no `+=` array building.
- Commit and PR titles use Conventional Commits with Gitmoji:
  `fix(certificates): 🐛 distinguish empty and unavailable custom fields`.

## Gotchas

- Live configuration comes from `SECTIGO_*` environment variables
  (`ApiConfigLoader`) or a credentials file. The test assembly clears them and
  redirects the token cache and credentials file to a temporary folder, so
  tests never reach a real tenant. Keep it that way; don't add live API tests.
- When the Admin API changes, download the new OpenAPI spec over
  `Documentation/api.json` rather than editing it.
