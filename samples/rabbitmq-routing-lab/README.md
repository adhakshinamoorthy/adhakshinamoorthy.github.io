# RabbitMQ Routing Lab

A deterministic .NET 10 simulator for RabbitMQ topic-exchange routing, queue fan-out, delivery, acknowledgement, and negative acknowledgement with requeue.

## Run
```powershell
dotnet run --project src/RabbitMqRoutingLab
```
## Test
```powershell
dotnet test RabbitMqRoutingLab.slnx
```

This teaches broker semantics without pretending to be RabbitMQ. Production verification must also use the official RabbitMQ .NET client against a real broker and cover durable topology, publisher confirms, prefetch, reconnect recovery, dead-letter exchanges, quorum queues, security, and node failure.
