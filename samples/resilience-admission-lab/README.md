# Resilience Admission Lab

A deterministic .NET 10 lab for bounded transient retries and token-bucket admission control with injectable time.

## Run
```powershell
dotnet run --project src/ResilienceAdmissionLab
```
## Test
```powershell
dotnet test ResilienceAdmissionLab.slnx
```

Production ASP.NET Core applications should prefer the built-in rate-limiting middleware and Microsoft.Extensions.Http.Resilience. This lab isolates the semantics so retry classification, attempt bounds, and admission behavior are directly testable.
