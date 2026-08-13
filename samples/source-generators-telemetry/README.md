# Source Generators Telemetry

A focused .NET 10 sample showing how to build, consume, inspect, and test an incremental C# source generator.

The generator discovers types marked with `[GenerateTelemetry]`, reads their public properties through Roslyn symbols, and emits a deterministic `ToTelemetry()` method. Invalid non-partial targets receive compiler diagnostic `TSG001`; unsupported nested or generic targets receive `TSG002`.

## What this proves

- `IIncrementalGenerator` and `ForAttributeWithMetadataName` create a narrow, cacheable discovery pipeline.
- The attribute is injected during post-initialization, so consumers need no separate contracts package for this small sample.
- Generated output is deterministic, readable, nullable-aware, and uses collision-resistant hint names.
- The generator is consumed as an analyzer with `ReferenceOutputAssembly="false"`.
- Tests assert generated source, diagnostics, determinism, and runtime use from the consuming application.
- Generated files can be inspected under `src/TelemetryDemo/obj/generated` after a build.

## Run

```powershell
dotnet restore SourceGeneratorsTelemetry.slnx
dotnet build SourceGeneratorsTelemetry.slnx --configuration Release --no-restore
dotnet run --project src/TelemetryDemo --configuration Release --no-build
```

## Test

```powershell
dotnet test SourceGeneratorsTelemetry.slnx --configuration Release --no-build --no-restore
```

The sample deliberately performs no file or network I/O from the generator. Production generators should treat compiler inputs as the complete, deterministic source of truth and use diagnostics for invalid consumer code.
