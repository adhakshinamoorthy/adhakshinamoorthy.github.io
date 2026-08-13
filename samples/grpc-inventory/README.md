# gRPC Inventory

A contract-first .NET 10 gRPC service demonstrating protobuf compatibility, unary and server-streaming calls, deadlines, cancellation, metadata authentication, status codes, validation, and in-process integration tests.

## Run

```powershell
dotnet run --project src/GrpcInventory
```

The service listens on the configured ASP.NET Core HTTPS/HTTP2 endpoint. Call it with a gRPC client and metadata `x-api-key: local-demo-key`; replace the development key through configuration outside source for real use.

## Test

```powershell
dotnet test GrpcInventory.slnx
```

The tests host the real HTTP/2 service in memory and verify authentication, validation status, unary lookup, streaming, and cancellation/deadline behavior.
