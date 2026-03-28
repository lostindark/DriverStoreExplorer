# Copilot Instructions for DriverStoreExplorer

## Build & Test

This is a .NET Framework 4.6.2 Windows Forms application. Use **MSBuild** (not `dotnet build`).

```powershell
# Restore + Build
nuget restore Rapr.sln
msbuild Rapr.sln /p:Configuration=Release /p:Platform="Any CPU" /v:minimal

# Run all tests (MSTest)
dotnet test RaprTests\RaprTests.csproj

# Run a single test
dotnet test RaprTests\RaprTests.csproj --filter "FullyQualifiedName~TestMethodName"
```

The CI workflow (`.github/workflows/ci.yml`) builds with MSBuild on `windows-latest`. There is no separate lint step — code analysis is handled by the `Microsoft.CodeAnalysis.NetAnalyzers` NuGet package at build time.

## Architecture

**Driver store abstraction:** The core pattern is `IDriverStore` (in `Rapr/Utils/`), which defines operations for enumerating, adding, deleting, and exporting driver packages. Three implementations exist:

- `NativeDriverStore` — Windows native API via P/Invoke (`SetupAPI.cs`, `SafeNativeMethods.cs`)
- `DismUtil` — DISM API via `Microsoft.Dism` NuGet package
- `PnpUtil` — Parses `pnputil.exe` command output

`DriverStoreFactory` creates the appropriate implementation based on user settings with automatic fallback based on OS capabilities. Each backend supports both online (local machine) and offline (mounted image) driver stores.

**Main form split:** The main UI is split across `DSEForm.cs` (form logic, event handlers, driver operations) and `DSEFormHelper.cs` (static helpers for OS detection, admin checks, path resolution, native interop).

**Single-file deployment:** NuGet dependencies are embedded as resources via `Resource.Embedder` and resolved at runtime through `AppDomain.CurrentDomain.AssemblyResolve` in `Program.cs`. This means the app ships as a single executable with embedded DLLs.

**Data model:** `DriverStoreEntry` is the central data class representing a driver package in the store. It holds metadata (INF name, version, signer, size, device association) and utility methods for display formatting. `DriverStoreRepository` handles mapping between `%SystemRoot%\INF` and the `DriverStore\FileRepository` by comparing INF file contents.

**Export pattern:** `IExport` interface with `CSVExporter` implementation. New export formats should implement this interface.

## Key Conventions

**Localization:** All user-facing strings go through `Rapr/Lang/Language.resx` (base English) with satellite `.resx` files per locale (e.g., `Language.fr-FR.resx`). Access strings via the generated `Language` class (e.g., `Language.Product_Name`). The base `Language.resx` uses `PublicResXFileCodeGenerator`. There are 20+ language files — never hardcode UI strings.

**WinForms designer pattern:** Forms and controls follow the standard `*.cs` / `*.Designer.cs` / `*.resx` triple. Don't hand-edit `*.Designer.cs` files.

**Settings:** User settings are defined in `Properties/Settings.settings` and accessed via `Properties.Settings.Default`. The app uses `PortableSettingsProvider` for portable config files alongside the executable.

**ListView:** The driver list uses `ObjectListView` (via `BrightIdeasSoftware` namespace), customized in `MyObjectListView.cs`. Use OLV's virtual mode and column aspects rather than raw `ListView` APIs.

**Code analysis:** `GlobalSuppressions.cs` contains project-level analyzer suppressions. The project uses `Microsoft.CodeAnalysis.NetAnalyzers` for static analysis during build.

**Strong naming:** The assembly is strong-named (`Rapr.snk`). Third-party assemblies are re-signed via the `StrongNamer` NuGet package.

**Namespace:** Main app code is in the `Rapr` namespace; utilities in `Rapr.Utils`; tests in `Rapr.Tests`.
