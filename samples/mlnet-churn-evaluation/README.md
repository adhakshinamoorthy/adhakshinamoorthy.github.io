# ML.NET Churn Evaluation

A .NET 10 churn-scoring lab with deterministic features, probability thresholding, confusion-matrix metrics, and a drift comparison.

## What it demonstrates

- repeatable .NET machine-learning pipelines for data loading, feature transformation, training, evaluation, persistence, and inference.
- ML.NET hosts the pipeline and model in .NET; data quality, label definition, fairness, drift response, and business decisions require product and domain ownership.
- A credential-free local path with deterministic output and a small self-check.

## Run

```powershell
dotnet run --project src/MlnetChurnEvaluation
```

## Check

```powershell
dotnet run --project src/MlnetChurnEvaluation -- --self-test
```

## Production boundary

Leakage, class imbalance, train-serving skew, stale features, or a threshold chosen without business cost can make an impressive offline metric harmful in production. Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.
