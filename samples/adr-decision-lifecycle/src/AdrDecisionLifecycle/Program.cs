var records = new[]
{
    new Adr(1, "Use PostgreSQL", "Superseded", "platform", 2),
    new Adr(2, "Use managed PostgreSQL", "Accepted", "platform", null),
    new Adr(3, "Adopt outbox", "Proposed", "orders", null)
};
var ids = records.Select(x => x.Id).ToHashSet();
var errors = records.Where(x => string.IsNullOrWhiteSpace(x.Owner) || (x.Status == "Superseded" && (!x.SupersededBy.HasValue || !ids.Contains(x.SupersededBy.Value)))).ToArray();
foreach (var adr in records) Console.WriteLine($"ADR-{adr.Id:D4} {adr.Status}: {adr.Title}");
if (args.Contains("--self-test") && errors.Length != 0) return 1;
return errors.Length == 0 ? 0 : 2;
sealed record Adr(int Id, string Title, string Status, string Owner, int? SupersededBy);
