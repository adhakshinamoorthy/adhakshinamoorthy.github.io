namespace ResilienceAdmissionLab;

public sealed class TokenBucket(int capacity, int refillPerSecond, TimeProvider time)
{ private double tokens = capacity; private DateTimeOffset last = time.GetUtcNow(); public bool TryAcquire() { var now = time.GetUtcNow(); tokens = Math.Min(capacity, tokens + (now - last).TotalSeconds * refillPerSecond); last = now; if (tokens < 1) return false; tokens--; return true; } }
public sealed class RetryPolicy(int maxAttempts, Func<int, TimeSpan> delay)
{ public async ValueTask<T> ExecuteAsync<T>(Func<int, CancellationToken, ValueTask<T>> action, Func<Exception, bool> transient, CancellationToken ct = default) { for (var attempt = 1; ; attempt++) { try { return await action(attempt, ct); } catch (Exception ex) when (transient(ex) && attempt < maxAttempts) { await Task.Delay(delay(attempt), ct); } } } }
public sealed class TransientException(string message) : Exception(message);
