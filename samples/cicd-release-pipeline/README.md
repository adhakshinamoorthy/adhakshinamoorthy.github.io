# CI/CD Release Pipeline

A .NET 10 release metadata tool and GitHub Actions workflow demonstrating fail-fast verification, dependency auditing, immutable artifacts, environment promotion, least-privilege permissions, and workload identity readiness.

## Run

```powershell
$env:RELEASE_VERSION='1.4.2'
$env:GIT_SHA='abcdef1234567'
$env:ARTIFACT_SHA256=('a' * 64)
dotnet run --project src/ReleaseManifest
```

## Test

```powershell
dotnet test CicdReleasePipeline.slnx
```

The workflow verifies pull requests and main, then publishes exactly one artifact for a version tag. The deployment job downloads that artifact instead of rebuilding it. Configure the `production` GitHub Environment with approvals and provider-specific OIDC trust before replacing the placeholder deployment command.
