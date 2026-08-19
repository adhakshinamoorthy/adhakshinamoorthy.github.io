using System.Collections.Concurrent;

namespace LogicAppsOrderApproval;

public sealed record ApprovalResult(string WorkflowRunId, Guid OrderId, string Decision, DateTimeOffset DecidedAt);
public sealed record ApprovalCallback(string WorkflowRunId, Guid OrderId, string Decision);

public sealed class ApprovalRegistry
{
    private readonly ConcurrentDictionary<string, ApprovalResult> _results = new(StringComparer.Ordinal);

    public (ApprovalResult Result, bool Created) Record(ApprovalCallback callback, DateTimeOffset now)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(callback.WorkflowRunId);
        if (callback.OrderId == Guid.Empty) throw new ArgumentException("OrderId is required.", nameof(callback));
        if (callback.Decision is not ("approved" or "rejected"))
            throw new ArgumentException("Decision must be approved or rejected.", nameof(callback));

        var candidate = new ApprovalResult(callback.WorkflowRunId, callback.OrderId, callback.Decision, now);
        var result = _results.GetOrAdd(callback.WorkflowRunId, candidate);
        return (result, ReferenceEquals(result, candidate));
    }

    public ApprovalResult? Find(string workflowRunId) => _results.GetValueOrDefault(workflowRunId);
}
