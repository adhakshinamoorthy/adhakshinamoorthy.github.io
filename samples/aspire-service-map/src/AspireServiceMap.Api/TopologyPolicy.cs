namespace AspireServiceMap.Api;

public sealed record ResourceReference(string Source, string Target);

public static class TopologyPolicy
{
    public static IReadOnlyList<string> Validate(IEnumerable<string> resources, IEnumerable<ResourceReference> references)
    {
        var known = resources.ToHashSet(StringComparer.OrdinalIgnoreCase);
        return references
            .Where(reference => !known.Contains(reference.Source) || !known.Contains(reference.Target) || reference.Source.Equals(reference.Target, StringComparison.OrdinalIgnoreCase))
            .Select(reference => $"Invalid reference: {reference.Source} -> {reference.Target}")
            .ToArray();
    }
}
