# WinGet Studio Development Guide

## Project Overview

**WinGet Studio** (experimental) is a Windows app for authoring and testing WinGet Configuration files (DSC-based Configuration as Code) without needing deep DSC expertise up front. It lets users create/edit configuration files and run "get", "set", and "test" against **installed** DSC resources.

- **`src/WinGetStudio`** - Main WinUI application (UI, views, view models)
- **`src/WinGetStudio.CLI`** - Command-line surface
- **`src/WinGetStudio.Core`** - Shared UI-adjacent helpers (`FileDialog`, `Helpers`)
- **`src/services/WinGetStudio.Services.Core`** - Core service abstractions
- **`src/services/WinGetStudio.Services.DesiredStateConfiguration`** / **`...Explorer`** - DSC resource discovery, invocation, and exploration services
- **`src/services/WinGetStudio.Services.Localization`** - Localization service
- **`src/services/WinGetStudio.Services.Logging`** - Logging service
- **`src/services/WinGetStudio.Services.Settings`** - App settings persistence
- **`src/services/WinGetStudio.Services.Telemetry`** - Telemetry service
- **`src/services/WinGetStudio.Services.VisualFeedback`** - UI feedback/animation helpers
- **`src/WinGetStudio.Tests.MSTest`** - MSTest-based unit tests
- **`docs/`** - User-facing documentation (getting started, DSC concepts, migration, CLI reference)

## Building, Testing, and Running

### Initial Setup

Apply the WinGet configuration file to provision your dev environment:

```powershell
winget configure '.\.config\configuration.winget'
```

### Building

- **Visual Studio**: open `src/WinGetStudio.sln` and build.
- **Script**: run `.\Build.ps1` from a PowerShell terminal (certificate signing requires admin privileges).

### Testing

Run tests via Visual Studio Test Explorer against `WinGetStudio.Tests.MSTest`, or `dotnet test` from the command line.

## Architecture & Key Patterns

- **Service-oriented structure**: cross-cutting concerns (DSC operations, settings, telemetry, logging, localization) are implemented as injectable services under `src/services/`, consumed by the WinUI app and CLI.
- **DSC v3 vs 0.2.0 format**: the app must handle both legacy WinGet Configuration (0.2.0) and Microsoft DSC v3.x configuration formats — see `docs/concepts/configuration-versions.md` and `docs/how-to/migrate-configuration-to-dsc3.md` before changing config parsing/serialization.
- **Resource discovery**: `WinGetStudio.Services.DesiredStateConfiguration.Explorer` surfaces installed DSC resources (including PowerShell Gallery-sourced ones) for the authoring UI.

## Naming Conventions

- Namespaces follow `WinGetStudio.<Area>` (e.g., `WinGetStudio.Services.DesiredStateConfiguration`).
- Shared build/versioning settings live in `Directory.Build.props`, `Directory.Packages.props`, and `ToolingVersions.props` at `src/` root — prefer updating these over per-project overrides.

## Contributing

- Review `CONTRIBUTING.md` (root) for workflow and CLA requirements.
- This project is **experimental** — expect API/format churn; check `docs/changelog.md` before assuming stability of a given surface.
- CI runs via Azure Pipelines.

## Issues and Pull Requests

- Before filing an issue, search existing open and closed issues for duplicates.
- Use the GitHub issue forms in `.github/ISSUE_TEMPLATE/`; do not file a blank issue unless a maintainer explicitly asks for one.
- Bug reports should include the form fields for brief description, steps to reproduce, expected behavior, actual behavior, and environment.
- Feature requests should include the form fields for feature or enhancement description and proposed technical implementation details when known.
- Keep issue bodies concise and evidence-based. Do not paste large speculative patches into issue bodies; open a pull request or link a branch when code is available.
- Before opening a pull request, review `CONTRIBUTING.md`, follow the PR template, keep the change focused, and summarize validation performed.

## Privacy & Telemetry

The application logs basic diagnostic telemetry. See `docs/` and `PRIVACY.md` for details on what is collected.
