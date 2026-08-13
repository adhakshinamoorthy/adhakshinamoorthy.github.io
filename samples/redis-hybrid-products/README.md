# Redis Hybrid Products

A .NET 10 API demonstrating `HybridCache` with Redis as a distributed L2 cache, in-process L1 caching, stampede protection, versioned keys, explicit invalidation, bounded serialization, local fallback, and integration tests.

## Run

For a production-shaped local run, start Redis and keep its connection string outside source:

```powershell
$env:ConnectionStrings__Redis = "localhost:6379"
dotnet run --project src/RedisHybridProducts
```

When no Redis connection is configured in Development, the sample uses `AddDistributedMemoryCache` so its contract remains runnable. That provider is process-local and is not a production substitute for Redis in a multi-instance deployment.

## Test

```powershell
dotnet test RedisHybridProducts.slnx
```

Tests prove cache hits, concurrent stampede protection, mutation invalidation, and not-found behavior without requiring external infrastructure.
