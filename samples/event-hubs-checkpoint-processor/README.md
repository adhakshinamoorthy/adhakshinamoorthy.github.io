# Event Hubs Checkpoint Processor

A .NET 10 local model of the mechanics behind an Azure Event Hubs consumer: stable partition keys, ordered per-partition sequence numbers, consumer checkpoints, bounded batches, cancellation, and an idempotent sink. The included Bicep provisions a real Event Hubs namespace, event hub, consumer group, and checkpoint storage account without connection-string authentication.

## Run locally

```powershell
dotnet run --project src/EventHubsCheckpointProcessor
```

Post `{"eventId":"event-1","deviceId":"device-42","value":21.5}` to `/events`. Use the returned partition ID with `POST /partitions/{partitionId}/process`, then inspect `/processor`. Events for the same device always select the same partition.

## Test and deploy

```powershell
dotnet test EventHubsCheckpointProcessor.slnx
az bicep build --file infra/main.bicep
```

For Azure, replace `LocalEventHub` with `EventHubProducerClient` and `EventProcessorClient`, use `DefaultAzureCredential`, create the checkpoint blob container, and grant narrowly scoped data sender/receiver and storage blob contributor roles. Keep a different consumer group and checkpoint container per logical application.

For production, choose partition count from long-term parallelism, use high-cardinality stable keys, batch sends, bound processor concurrency to downstream capacity, checkpoint only after durable side effects, preserve idempotency across replay, quarantine poison events, monitor ingress/egress throttling and consumer lag, enable Capture for durable archive, and rehearse retention-window recovery.
