# Vertical Slice Support

A runnable .NET 10 support-ticket example organized by behavior. `CreateTicket` and `GetTicket` each keep their request, response, validation, authorization-sensitive query, and handler together instead of spreading one change across controller/service/repository folders.

## What it demonstrates

- A thin request/handler contract without a mediator dependency.
- Slice-local input validation and output mapping.
- Customer-scoped lookup that returns no information for a cross-customer ID.
- Tests written at the behavior boundary of each slice.
- A small shared store representing infrastructure that multiple slices deliberately reuse.

## Run

```powershell
dotnet run --project src/VerticalSliceSupport
```

## Test

```powershell
dotnet test VerticalSliceSupport.slnx
```

Vertical slices are not permission to duplicate everything. Share stable infrastructure and cross-cutting policies deliberately; keep business behavior close to the feature that changes it.
