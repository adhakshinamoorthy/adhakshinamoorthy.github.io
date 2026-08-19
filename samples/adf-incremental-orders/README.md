# ADF Incremental Orders

A .NET 10 local model of a restartable Azure Data Factory watermark pipeline. It snapshots a source range, validates the batch, upserts an idempotent curated sink, and advances the durable watermark only after successful writes. The repository also contains a source-controlled ADF pipeline definition and Bicep for a managed-identity factory and secure landing storage.

## Run and test

```powershell
dotnet run --project src/AdfIncrementalOrders
dotnet test AdfIncrementalOrders.slnx
az bicep build --file infra/main.bicep
```

Post changes such as `{"orderId":"o-1","total":42.50,"watermark":1}` to `/source/orders`, call `POST /pipeline/run`, then inspect `/pipeline`. Re-running without new source rows writes nothing; a newer change for the same order safely replaces the curated value.

In Azure, keep linked-service credentials out of JSON, grant the factory identity only the required data roles, use private connectivity where required, store the committed watermark separately from the in-flight high watermark, and reconcile counts/schema/freshness before committing progress. Monitor data quality as well as activity success.
