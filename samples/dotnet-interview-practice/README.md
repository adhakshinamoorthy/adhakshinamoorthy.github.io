# .NET Interview Practice

A .NET 10 interview-practice engine that schedules prompts, scores answers against a transparent architecture rubric, and prioritizes weak competencies.

## What it demonstrates

- preparing evidence-based .NET architecture answers that explain context, trade-offs, implementation, failure handling, verification, and measured outcomes.
- Interview preparation organizes knowledge and practice; strong answers remain honest about personal experience, uncertainty, alternatives, and the actual result.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/DotnetInterviewPractice
```

## Check

```powershell
dotnet run --project src/DotnetInterviewPractice -- --self-test
```

## Production boundary

Memorized definitions without context, trade-offs, failure modes, or evidence sound shallow and collapse under follow-up questions. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
