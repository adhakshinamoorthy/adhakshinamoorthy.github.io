# Kafka Partition Lab

A deterministic .NET 10 simulator of Kafka's partitioned append-only log, key-based ordering, independent consumer-group offsets, redelivery before commit, and resumption after commit.

## Run
```powershell
dotnet run --project src/KafkaPartitionLab
```
## Test
```powershell
dotnet test KafkaPartitionLab.slnx
```

This is a semantics lab, not a Kafka replacement. Production work must use a supported client against a real cluster and validate serialization/schema compatibility, acknowledgements, idempotent production, rebalances, offset commits, poison records, retention, replication, security, capacity, and disaster recovery.
