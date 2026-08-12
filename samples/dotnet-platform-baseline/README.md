# .NET Platform Baseline

A focused .NET 10 console application that turns a JSON work manifest into a deterministic report while demonstrating the platform practices that make builds and deployments repeatable.

## What this sample demonstrates

- SDK selection through `global.json` and a `net10.0` target framework
- repository-wide compiler, nullable, analyzer, deterministic-build, and central-package settings
- Generic Host configuration from JSON, environment variables, and command-line arguments
- validated options, dependency injection, structured logging, and `TimeProvider`
- asynchronous file I/O, cancellation, SHA-256, UTF-8, and atomic output replacement using the base class libraries
- source-generated `System.Text.Json` metadata that is friendly to trimming and native compilation
- platform and runtime inspection through `RuntimeInformation`
- deterministic tests using real temporary files

## Run

From this directory:

```powershell
dotnet restore DotnetPlatformBaseline.slnx
dotnet run --project src/DotnetPlatformBaseline
```

Override configuration without changing source:

```powershell
dotnet run --project src/DotnetPlatformBaseline -- `
  --Processing:InputPath=src/DotnetPlatformBaseline/examples/work-items.json `
  --Processing:OutputPath=artifacts/custom-report.json
```

Environment variables use double underscores for nested keys:

```powershell
$env:Processing__MaximumItems = "500"
dotnet run --project src/DotnetPlatformBaseline
```

## Test and publish

```powershell
dotnet test DotnetPlatformBaseline.slnx --configuration Release
dotnet publish src/DotnetPlatformBaseline --configuration Release --output artifacts/publish
dotnet artifacts/publish/DotnetPlatformBaseline.dll
```

For a self-contained artifact, choose and test an explicit runtime identifier:

```powershell
dotnet publish src/DotnetPlatformBaseline --configuration Release `
  --runtime win-x64 --self-contained true --output artifacts/win-x64
```

Framework-dependent output is smaller and receives runtime servicing from the machine or container. Self-contained output carries its runtime, increases artifact size, and makes runtime patching part of the application release process.

## Expected result

The default run writes `artifacts/work-report.json` relative to the process working directory. The report includes the batch, UTC timestamp, runtime/OS/architecture details, UTF-8 payload lengths, and SHA-256 values.

The sample has no external service or database dependency.
