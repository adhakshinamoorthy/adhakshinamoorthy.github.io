using HealthReadinessLab;
var state = new StartupState { Ready = true }; var results = await new HealthEvaluator([new StartupProbe(state), new DependencyProbe(_ => ValueTask.FromResult(true))]).CheckAsync(); Console.WriteLine($"Readiness={HealthEvaluator.Aggregate(results)}; checks={results.Count}");
