using ResilienceAdmissionLab;
var policy = new RetryPolicy(3, _ => TimeSpan.Zero); var value = await policy.ExecuteAsync<string>((attempt, _) => attempt < 3 ? ValueTask.FromException<string>(new TransientException("busy")) : ValueTask.FromResult("ok"), ex => ex is TransientException); Console.WriteLine($"Result={value}; admission={new TokenBucket(1, 0, TimeProvider.System).TryAcquire()}");
