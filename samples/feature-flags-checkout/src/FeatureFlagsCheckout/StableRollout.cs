using System.Security.Cryptography;
using System.Text;

namespace FeatureFlagsCheckout;

public sealed record CheckoutFlag(string Name, bool Enabled, int Percentage, string[] IncludedGroups, string Owner, DateOnly ReviewBy);
public sealed record FlagDecision(string Flag, bool Enabled, string Reason, int Bucket, string Owner, DateOnly ReviewBy);

public sealed class StableRolloutEvaluator(CheckoutFlag flag)
{
    public FlagDecision Evaluate(string userId, IEnumerable<string> groups)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        if (!flag.Enabled) return new(flag.Name, false, "globally-disabled", Bucket(flag.Name, userId), flag.Owner, flag.ReviewBy);
        var groupSet = groups.ToHashSet(StringComparer.OrdinalIgnoreCase);
        if (flag.IncludedGroups.Any(groupSet.Contains)) return new(flag.Name, true, "targeted-group", Bucket(flag.Name, userId), flag.Owner, flag.ReviewBy);
        var bucket = Bucket(flag.Name, userId);
        return new(flag.Name, bucket < Math.Clamp(flag.Percentage, 0, 100), "percentage", bucket, flag.Owner, flag.ReviewBy);
    }

    public static int Bucket(string flagName, string userId)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{flagName}:{userId}"));
        return (int)(BitConverter.ToUInt32(hash, 0) % 100u);
    }
}
