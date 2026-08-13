# SignalR Operations Room

A .NET 10 real-time sample with authenticated connections, strongly typed hub contracts, server-controlled room membership, bounded messages, reconnect-friendly snapshots, targeted broadcasts, and end-to-end SignalR client tests.

## Run

```powershell
dotnet run --project src/SignalROperationsRoom
```

For local demonstration, connect to `/hubs/operations` with an `X-User-Id` header. This intentionally simple scheme shows the authentication boundary; use your approved cookie or bearer identity provider in production.

## Test

```powershell
dotnet test SignalROperationsRoom.slnx
```

Tests negotiate and connect through the real SignalR client, prove anonymous denial, receive a room snapshot, broadcast a message, and reject invalid rooms.
