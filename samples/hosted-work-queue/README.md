# Hosted Work Queue

A runnable .NET 10 bounded Channel work queue demonstrating backpressure, cooperative cancellation, per-item failure isolation, and graceful drain after the writer completes.

## Run
```powershell
dotnet run --project src/HostedWorkQueue
```
## Test
```powershell
dotnet test HostedWorkQueue.slnx
```

An in-memory queue loses pending work on process failure and cannot coordinate replicas. Use a durable broker or job store when work must survive restart, and create a dependency-injection scope per item in a real BackgroundService.
