import { mkdir, readFile, writeFile } from 'node:fs/promises';
import { execFileSync } from 'node:child_process';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';

const root = join(dirname(fileURLToPath(import.meta.url)), '..');
const today = '2026-08-15';

const specs = [
  {
    batch: 17, slug: 'semantic-kernel', sampleId: 'semantic-kernel-order-agent', project: 'SemanticKernelOrderAgent',
    sampleName: 'Semantic Kernel Order Agent', focus: 'controlled AI orchestration through small, well-described plugins, grounded context, and explicit approval before side effects',
    boundary: 'The kernel coordinates models and application functions; domain services still own authorization, validation, transactions, and audit.',
    failure: 'A model can choose an irrelevant tool, invent arguments, expose sensitive context, or repeat an action after a timeout.',
    extras: ['Function-choice control', 'Grounding and evaluation'],
    practices: ['Separate read-only retrieval tools from state-changing tools and require confirmation for consequential work.', 'Evaluate tool selection, argument validity, grounding, refusal, latency, and token cost with a versioned test set.'],
    questions: [['Why should authorization remain inside a Semantic Kernel plugin dependency?', 'Model tool selection is not a trust decision. The called application service must authorize the authenticated actor and resource every time.'], ['How do you make an AI workflow replay-safe?', 'Give actions stable operation identifiers, persist their state, make handlers idempotent, and return the existing result when a request is repeated.']],
    sampleDescription: 'A credential-free .NET 10 plugin orchestrator that separates retrieval from side effects, validates tool arguments, requires approval, and records idempotent operation IDs.',
    code: `using System.Text.Json;

var orders = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase) { ["ORD-42"] = "Packed" };
var completed = new Dictionary<string, string>();

string GetStatus(string orderId) => orders.TryGetValue(orderId, out var status) ? status : "Not found";
string Cancel(string orderId, string operationId, bool approved)
{
    if (!approved) return "Approval required";
    if (completed.TryGetValue(operationId, out var prior)) return prior;
    if (!orders.ContainsKey(orderId)) return "Order not found";
    orders[orderId] = "Cancelled";
    return completed[operationId] = $"{orderId} cancelled";
}

Console.WriteLine(JsonSerializer.Serialize(new { tool = "get_order_status", result = GetStatus("ORD-42") }));
Console.WriteLine(JsonSerializer.Serialize(new { tool = "cancel_order", result = Cancel("ORD-42", "OP-100", approved: false) }));
Console.WriteLine(JsonSerializer.Serialize(new { tool = "cancel_order", result = Cancel("ORD-42", "OP-100", approved: true) }));
if (args.Contains("--self-test") && (GetStatus("ORD-42") != "Cancelled" || completed.Count != 1)) return 1;
return 0;`
  },
  {
    batch: 17, slug: 'microsoft-extensions-ai', sampleId: 'extensions-ai-chat-pipeline', project: 'ExtensionsAiChatPipeline',
    sampleName: 'Extensions AI Chat Pipeline', focus: 'provider-neutral chat and embedding abstractions with composable middleware for telemetry, caching, tool use, and resilience',
    boundary: 'Microsoft.Extensions.AI standardizes application-facing AI contracts; provider SDKs, model behavior, data governance, and product policy remain explicit choices.',
    failure: 'A provider swap can change tokenization, tool calling, safety behavior, latency, limits, or response shape even when the application interface stays stable.',
    extras: ['Client middleware', 'Provider portability'],
    practices: ['Keep provider-specific options at the composition root and expose stable application-level requests and results.', 'Record model, prompt, tool, latency, token, cache, safety, and evaluation metadata without logging sensitive prompt content.'],
    questions: [['What portability does IChatClient provide?', 'It makes common chat operations and middleware portable, but it does not guarantee equivalent model quality, limits, tool behavior, or pricing.'], ['Why use a delegating chat client pipeline?', 'Cross-cutting behavior such as caching, telemetry, retries, and tool invocation can be composed and tested without polluting business code.']],
    sampleDescription: 'A .NET 10 provider-neutral chat client pipeline demonstrating delegating middleware, deterministic caching, timing, and a swappable local provider.',
    code: `using System.Diagnostics;

IChatClient client = new TimingClient(new CacheClient(new LocalChatClient()));
foreach (var prompt in new[] { "summarize order 42", "summarize order 42", "explain retry policy" })
    Console.WriteLine(await client.CompleteAsync(prompt));

if (args.Contains("--self-test") && CacheClient.Hits != 1) return 1;
return 0;

interface IChatClient { Task<string> CompleteAsync(string prompt); }
sealed class LocalChatClient : IChatClient
{
    public Task<string> CompleteAsync(string prompt) => Task.FromResult($"local:{prompt.ToUpperInvariant()}");
}
sealed class CacheClient(IChatClient inner) : IChatClient
{
    private readonly Dictionary<string, string> cache = new(StringComparer.Ordinal);
    public static int Hits { get; private set; }
    public async Task<string> CompleteAsync(string prompt)
    {
        if (cache.TryGetValue(prompt, out var value)) { Hits++; return $"cache:{value}"; }
        return cache[prompt] = await inner.CompleteAsync(prompt);
    }
}
sealed class TimingClient(IChatClient inner) : IChatClient
{
    public async Task<string> CompleteAsync(string prompt)
    {
        var timer = Stopwatch.StartNew();
        var value = await inner.CompleteAsync(prompt);
        return $"{value} ({timer.ElapsedMilliseconds} ms)";
    }
}`
  },
  {
    batch: 17, slug: 'model-context-protocol', sampleId: 'mcp-inventory-tools', project: 'McpInventoryTools',
    sampleName: 'MCP Inventory Tools', focus: 'interoperable discovery and invocation of tools, resources, and prompts over a capability-negotiated JSON-RPC protocol',
    boundary: 'MCP describes how a host, client, and server exchange capabilities; the server remains responsible for identity, authorization, validation, rate limits, and safe tool semantics.',
    failure: 'An overpowered tool, confused-deputy flow, untrusted server, prompt injection, or unbounded result can turn useful model context into a security incident.',
    extras: ['Capability negotiation', 'Tool safety boundaries'],
    practices: ['Expose narrowly scoped tools with explicit JSON schemas, bounded output, read-only defaults, and clear side-effect descriptions.', 'Treat every MCP server and returned content as untrusted; apply user consent, allowlists, authorization, and audit at the host and server.'],
    questions: [['What roles participate in MCP?', 'A host owns the user experience and policy, a client maintains a connection, and a server exposes negotiated tools, resources, or prompts.'], ['Why is tool discovery not authorization?', 'A listed tool only describes availability and schema. Each invocation still needs authenticated identity, policy checks, validation, and safe execution.']],
    sampleDescription: 'A .NET 10 line-oriented JSON-RPC MCP teaching server that implements initialize, tools/list, and a schema-validated inventory tool without external services.',
    code: `using System.Text.Json;
using System.Text.Json.Nodes;

var request = JsonNode.Parse(args.FirstOrDefault(a => a.StartsWith('{')) ?? "{\"jsonrpc\":\"2.0\",\"id\":1,\"method\":\"tools/list\"}")!.AsObject();
var method = request["method"]?.GetValue<string>();
object result = method switch
{
    "initialize" => new { protocolVersion = "2025-11-25", capabilities = new { tools = new { } }, serverInfo = new { name = "inventory-tools", version = "1.0" } },
    "tools/list" => new { tools = new[] { new { name = "get_inventory", description = "Read stock for one SKU", inputSchema = new { type = "object", required = new[] { "sku" } } } } },
    "tools/call" => CallTool(request["params"] as JsonObject),
    _ => new { error = "Method not found" }
};
Console.WriteLine(JsonSerializer.Serialize(new { jsonrpc = "2.0", id = request["id"], result }));
if (args.Contains("--self-test") && method != "tools/list") return 1;
return 0;

static object CallTool(JsonObject? parameters)
{
    if (parameters?["name"]?.GetValue<string>() != "get_inventory") return new { isError = true, content = "Tool not allowed" };
    var sku = parameters["arguments"]?["sku"]?.GetValue<string>();
    return string.IsNullOrWhiteSpace(sku) || sku.Length > 32
        ? new { isError = true, content = "Invalid sku" }
        : new { isError = false, content = new { sku, available = 12 } };
}`
  },
  {
    batch: 18, slug: 'mlnet', sampleId: 'mlnet-churn-evaluation', project: 'MlnetChurnEvaluation', sampleName: 'ML.NET Churn Evaluation',
    focus: 'repeatable .NET machine-learning pipelines for data loading, feature transformation, training, evaluation, persistence, and inference',
    boundary: 'ML.NET hosts the pipeline and model in .NET; data quality, label definition, fairness, drift response, and business decisions require product and domain ownership.',
    failure: 'Leakage, class imbalance, train-serving skew, stale features, or a threshold chosen without business cost can make an impressive offline metric harmful in production.',
    extras: ['Pipeline reproducibility', 'Threshold and drift management'], practices: ['Split data by the way predictions will be made in production and prevent future information from leaking into training.', 'Version data, transforms, trainer settings, model artifact, evaluation results, threshold, and deployment metadata together.'],
    questions: [['Why is accuracy often misleading for churn?', 'When churn is rare, predicting no churn can be highly accurate. Precision, recall, PR curves, calibration, and business cost reveal usefulness.'], ['What is train-serving skew?', 'The training and production feature logic differ, so the deployed model receives values with different meaning or distribution.']],
    sampleDescription: 'A .NET 10 churn-scoring lab with deterministic features, probability thresholding, confusion-matrix metrics, and a drift comparison.',
    code: `var rows = new[] { new Customer(3, 1, true), new(18, 8, false), new(5, 3, true), new(30, 12, false) };
var threshold = 0.55;
var predictions = rows.Select(x => new { x.Churned, Score = Sigmoid(1.8 - .09 * x.MonthsActive + .35 * x.SupportTickets) }).ToArray();
var tp = predictions.Count(x => x.Churned && x.Score >= threshold);
var fp = predictions.Count(x => !x.Churned && x.Score >= threshold);
var fn = predictions.Count(x => x.Churned && x.Score < threshold);
var precision = tp / (double)Math.Max(1, tp + fp);
var recall = tp / (double)Math.Max(1, tp + fn);
Console.WriteLine($"threshold={threshold:F2} precision={precision:F2} recall={recall:F2}");
if (args.Contains("--self-test") && (precision < .5 || recall < .5)) return 1;
return 0;
static double Sigmoid(double value) => 1 / (1 + Math.Exp(-value));
sealed record Customer(int MonthsActive, int SupportTickets, bool Churned);`
  },
  {
    batch: 18, slug: 'dotnet-maui', sampleId: 'maui-offline-order-sync', project: 'MauiOfflineOrderSync', sampleName: '.NET MAUI Offline Order Sync',
    focus: 'one cross-platform .NET application with native lifecycle, adaptive UI, local persistence, secure platform integration, and resilient synchronization',
    boundary: '.NET MAUI shares UI and application code while platform projects, permissions, packaging, accessibility, lifecycle, and store policy remain platform-specific.',
    failure: 'Assuming permanent connectivity or identical platform behavior causes data loss, frozen UI, broken navigation, permission failures, and rejected releases.',
    extras: ['Offline-first state', 'Platform lifecycle and accessibility'], practices: ['Design local state and an idempotent sync queue before adding remote calls; expose pending, failed, and conflict states to the user.', 'Keep domain and synchronization logic independent of pages so it can be tested without device emulators.'],
    questions: [['What belongs in shared MAUI code?', 'View models, domain logic, validation, navigation contracts, and most UI can be shared; platform APIs and packaging remain behind platform-specific implementations.'], ['How should offline conflicts be handled?', 'Use version metadata and explicit merge policy, preserve the local intent, and ask the user when the business rule cannot resolve safely.']],
    sampleDescription: 'A .NET 10 offline-order domain core suitable for a MAUI app, with a durable-style outbox, idempotent synchronization, retry state, and conflict visibility.',
    code: `var store = new OfflineStore();
store.Save(new Order("ORD-7", 1, "Draft"));
store.Queue("ORD-7", "Submit", "OP-1");
var first = store.Sync("OP-1", remoteVersion: 1);
var duplicate = store.Sync("OP-1", remoteVersion: 1);
Console.WriteLine($"first={first} duplicate={duplicate} pending={store.PendingCount}");
if (args.Contains("--self-test") && (first != "Synced" || duplicate != "Already synced")) return 1;
return 0;

sealed record Order(string Id, int Version, string Status);
sealed class OfflineStore
{
    private readonly Dictionary<string, Order> orders = [];
    private readonly Dictionary<string, (string OrderId, string Action)> outbox = [];
    private readonly HashSet<string> completed = [];
    public int PendingCount => outbox.Count;
    public void Save(Order order) => orders[order.Id] = order;
    public void Queue(string orderId, string action, string operationId) => outbox[operationId] = (orderId, action);
    public string Sync(string operationId, int remoteVersion)
    {
        if (completed.Contains(operationId)) return "Already synced";
        if (!outbox.TryGetValue(operationId, out var item)) return "Missing operation";
        if (orders[item.OrderId].Version != remoteVersion) return "Conflict";
        completed.Add(operationId); outbox.Remove(operationId); return "Synced";
    }
}`
  },
  {
    batch: 18, slug: 'backend-for-frontend', sampleId: 'bff-dashboard-composition', project: 'BffDashboardComposition', sampleName: 'BFF Dashboard Composition',
    focus: 'a client-specific backend that composes downstream capabilities, protects browser credentials, and shapes contracts around one user experience',
    boundary: 'A BFF owns client orchestration and presentation-shaped contracts; domain rules and source-of-truth data remain in downstream services.',
    failure: 'An oversized BFF becomes a second monolith, duplicates business logic, fans out without deadlines, or exposes cookies and tokens through weak browser controls.',
    extras: ['Client-shaped composition', 'Token and cookie boundary'], practices: ['Use one BFF when a client has distinct composition, security, or release needs; do not create one per screen.', 'Propagate cancellation and deadlines, bound fan-out, cache only safe user-scoped results, and return partial data deliberately.'],
    questions: [['Why can a browser BFF reduce token exposure?', 'The browser can hold a protected same-site session cookie while the server-side BFF stores or exchanges access tokens away from JavaScript.'], ['Where should business rules live?', 'In domain services or application modules. The BFF should compose and reshape outcomes, not become another source of truth.']],
    sampleDescription: 'A .NET 10 dashboard composer with parallel bounded calls, user-scoped caching, partial-failure reporting, and a presentation-specific response.',
    code: `var composer = new DashboardComposer();
var view = await composer.LoadAsync("user-17", TimeSpan.FromSeconds(1));
Console.WriteLine($"orders={view.OrderCount} stock={view.LowStockCount} warnings={string.Join(',', view.Warnings)}");
if (args.Contains("--self-test") && view.OrderCount != 3) return 1;
return 0;

sealed record Dashboard(int OrderCount, int LowStockCount, string[] Warnings);
sealed class DashboardComposer
{
    public async Task<Dashboard> LoadAsync(string userId, TimeSpan timeout)
    {
        using var cts = new CancellationTokenSource(timeout);
        var orders = GetOrders(userId, cts.Token);
        var stock = GetStock(cts.Token);
        await Task.WhenAll(orders, stock);
        return new(await orders, await stock, []);
    }
    static async Task<int> GetOrders(string userId, CancellationToken ct) { await Task.Delay(10, ct); return 3; }
    static async Task<int> GetStock(CancellationToken ct) { await Task.Delay(10, ct); return 2; }
}`
  },
  {
    batch: 19, slug: 'solution-architecture-fundamentals', sampleId: 'architecture-quality-attributes', project: 'ArchitectureQualityAttributes', sampleName: 'Architecture Quality Attribute Workshop',
    focus: 'turning business goals and constraints into measurable quality-attribute scenarios, explicit trade-offs, bounded system responsibilities, and evolutionary decisions',
    boundary: 'Solution architecture connects product, software, data, integration, security, infrastructure, and operations; it does not replace detailed design or accountable delivery teams.',
    failure: 'A diagram-led design can look complete while omitting measurable availability, latency, security, recovery, cost, ownership, and change scenarios.',
    extras: ['Quality-attribute scenarios', 'Trade-off analysis'], practices: ['Write stimulus, environment, response, and measurable response before choosing a tactic or product.', 'Maintain context, container, deployment, data-flow, and trust-boundary views only when each answers a stakeholder question.'],
    questions: [['What makes a quality attribute actionable?', 'A concrete stimulus, operating condition, affected component, expected response, and measurable response target.'], ['How should an architect handle conflicting qualities?', 'Make the conflict and business priority visible, compare options using evidence, record the decision, and measure the chosen compromise.']],
    sampleDescription: 'A .NET 10 workshop model that validates quality-attribute scenarios, ranks architectural risk, and maps measurable responses to candidate tactics.',
    code: `var scenarios = new[]
{
    new Scenario("Reliability", "region unavailable", "peak", "checkout", "recover", 15, 9),
    new Scenario("Performance", "500 requests/s", "normal", "catalog", "respond", 250, 7),
    new Scenario("Security", "stolen credential", "any", "admin API", "deny and alert", 5, 10)
};
foreach (var item in scenarios.OrderByDescending(x => x.Risk * (1000d / x.Target)))
    Console.WriteLine($"{item.Attribute}: {item.Response} within {item.Target} ms/min; risk={item.Risk}");
if (args.Contains("--self-test") && scenarios.Any(x => x.Target <= 0 || x.Risk is < 1 or > 10)) return 1;
return 0;
sealed record Scenario(string Attribute, string Stimulus, string Environment, string Artifact, string Response, int Target, int Risk);`
  },
  {
    batch: 19, slug: 'architecture-decision-records', sampleId: 'adr-decision-lifecycle', project: 'AdrDecisionLifecycle', sampleName: 'ADR Decision Lifecycle',
    focus: 'capturing significant architectural decisions with context, options, consequences, status, evidence, and supersession history close to the code',
    boundary: 'An ADR records why a consequential choice was made; it complements diagrams, standards, code, and operational evidence rather than duplicating them.',
    failure: 'Decision records become ceremony when they are vague, retrospective, disconnected from changes, or silently edited after teams depend on them.',
    extras: ['Decision status lifecycle', 'Consequences and evidence'], practices: ['Create an ADR when a decision is costly to reverse, crosses teams, changes a quality attribute, or constrains future work.', 'Supersede rather than rewrite accepted history, and link the replacement, implementation, measurements, and review trigger.'],
    questions: [['What belongs in an ADR?', 'The decision context, forces, considered options, outcome, consequences, status, owners, and links to evidence or follow-up work.'], ['When should an ADR be superseded?', 'When the decision changes materially; retain the old record and link both directions so the history remains trustworthy.']],
    sampleDescription: 'A .NET 10 ADR catalog validator that detects missing owners, invalid status transitions, and accepted decisions whose superseding record is absent.',
    code: `var records = new[]
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
sealed record Adr(int Id, string Title, string Status, string Owner, int? SupersededBy);`
  },
  {
    batch: 19, slug: 'legacy-modernization', sampleId: 'legacy-strangler-router', project: 'LegacyStranglerRouter', sampleName: 'Legacy Strangler Router',
    focus: 'modernizing capability by capability using discovery, characterization tests, seams, incremental routing, data migration, observability, and reversible cutovers',
    boundary: 'Modernization changes business capability delivery and operational risk; a framework upgrade or cloud move alone does not remove coupling, unsafe data ownership, or weak delivery practices.',
    failure: 'A big-bang rewrite can spend years reproducing undocumented behavior while the legacy system keeps changing and business feedback arrives too late.',
    extras: ['Strangler migration', 'Behavior characterization'], practices: ['Inventory business capabilities, dependencies, data, change frequency, incidents, and business criticality before selecting a migration slice.', 'Route a measurable cohort through the new path, compare outcomes, preserve rollback, and retire the old path only after evidence and data reconciliation.'],
    questions: [['What is the strangler pattern?', 'New capability is built beside the legacy system and traffic moves incrementally through a routing seam until the old implementation can be retired.'], ['Why write characterization tests?', 'They capture behavior the business currently depends on, including quirks, so modernization changes can distinguish intentional improvement from accidental regression.']],
    sampleDescription: 'A .NET 10 cohort router that moves deterministic traffic from a legacy implementation to a modern slice, compares outcomes, and supports immediate rollback.',
    code: `var router = new StranglerRouter(30);
var requests = Enumerable.Range(1, 20).Select(i => $"customer-{i}").ToArray();
var routes = requests.GroupBy(router.Route).ToDictionary(x => x.Key, x => x.Count());
Console.WriteLine($"legacy={routes.GetValueOrDefault("legacy")} modern={routes.GetValueOrDefault("modern")}");
router.Rollback();
if (args.Contains("--self-test") && requests.Any(x => router.Route(x) != "legacy")) return 1;
return 0;
sealed class StranglerRouter(int percentage)
{
    private int percentage = Math.Clamp(percentage, 0, 100);
    public string Route(string key) => Math.Abs(StringComparer.Ordinal.GetHashCode(key)) % 100 < percentage ? "modern" : "legacy";
    public void Rollback() => percentage = 0;
}`
  },
  {
    batch: 20, slug: 'internal-developer-platforms', sampleId: 'platform-service-catalog', project: 'PlatformServiceCatalog', sampleName: 'Platform Service Catalog',
    focus: 'a product-managed platform that gives teams self-service golden paths for creation, delivery, security, observability, ownership, and lifecycle management',
    boundary: 'An internal developer platform reduces repeated cognitive load through paved roads and APIs; it is not merely a portal, a Kubernetes cluster, or a centralized ticket queue.',
    failure: 'A platform built without user research becomes mandatory infrastructure that shifts toil, hides unsafe defaults, and cannot demonstrate improved delivery outcomes.',
    extras: ['Platform as a product', 'Golden paths and scorecards'], practices: ['Measure developer journeys such as service creation, first deployment, recovery, and dependency upgrade before choosing platform features.', 'Make the secure observable path the easiest path, preserve documented escape hatches, and version platform contracts like any other product.'],
    questions: [['What is a golden path?', 'A supported, automated route for a common developer journey that embeds organizational defaults while allowing deliberate exceptions.'], ['How do you measure platform value?', 'Use adoption and satisfaction with delivery lead time, failure recovery, cognitive load, security posture, and journey completion—not portal page views alone.']],
    sampleDescription: 'A .NET 10 service-catalog validator that checks ownership, tier, repository, runtime, health, telemetry, and runbook metadata before onboarding.',
    code: `var services = new[]
{
    new Service("orders", "commerce", 1, "https://github.com/example/orders", true, true, true),
    new Service("catalog", "commerce", 2, "https://github.com/example/catalog", true, true, true)
};
var errors = services.SelectMany(Validate).ToArray();
foreach (var service in services) Console.WriteLine($"{service.Name}: owner={service.Owner} tier={service.Tier}");
if (args.Contains("--self-test") && errors.Length != 0) return 1;
return errors.Length == 0 ? 0 : 2;
static IEnumerable<string> Validate(Service s)
{
    if (string.IsNullOrWhiteSpace(s.Owner)) yield return $"{s.Name}: owner missing";
    if (s.Tier is < 1 or > 4) yield return $"{s.Name}: invalid tier";
    if (!s.Health || !s.Telemetry || !s.Runbook) yield return $"{s.Name}: operational metadata incomplete";
}
sealed record Service(string Name, string Owner, int Tier, string Repository, bool Health, bool Telemetry, bool Runbook);`
  },
  {
    batch: 20, slug: 'cloud-adoption-framework', sampleId: 'caf-landing-zone-readiness', project: 'CafLandingZoneReadiness', sampleName: 'CAF Landing Zone Readiness',
    focus: 'aligning cloud strategy, planning, landing zones, governance, security, operations, workload migration, and measurable business outcomes',
    boundary: 'The Cloud Adoption Framework supplies guidance and decision structure; each organization must tailor it to regulations, operating model, skills, portfolio, risk appetite, and workload needs.',
    failure: 'Migrating workloads before identity, connectivity, policy, ownership, budgets, logging, recovery, and support are ready reproduces legacy risk at cloud speed.',
    extras: ['Landing-zone readiness', 'Adoption and governance operating model'], practices: ['Separate platform landing-zone ownership from workload subscription ownership while agreeing interfaces, policies, support, and escalation.', 'Use policy and automation to prevent or quickly detect drift, but test effects and provide a governed exception lifecycle.'],
    questions: [['What is an Azure landing zone?', 'A scalable environment and operating model for identity, resource organization, networking, security, governance, management, and workload subscriptions.'], ['Why should strategy precede migration?', 'Business outcomes, constraints, portfolio priorities, and economics determine which workloads move, modernize, replace, retain, or retire.']],
    sampleDescription: 'A .NET 10 landing-zone readiness assessment that scores identity, organization, network, policy, operations, security, cost, and recovery gates.',
    code: `var controls = new[]
{
    new Control("Identity", true, 10), new("Resource organization", true, 8), new("Network", true, 9),
    new("Policy", true, 10), new("Operations", true, 9), new("Cost", false, 7), new("Recovery", true, 10)
};
var score = controls.Where(x => x.Ready).Sum(x => x.Weight) * 100d / controls.Sum(x => x.Weight);
foreach (var gap in controls.Where(x => !x.Ready)) Console.WriteLine($"gap={gap.Area} weight={gap.Weight}");
Console.WriteLine($"readiness={score:F0}%");
if (args.Contains("--self-test") && score is <= 0 or >= 100) return 1;
return 0;
sealed record Control(string Area, bool Ready, int Weight);`
  },
  {
    batch: 20, slug: 'aws-for-dotnet', sampleId: 'aws-dotnet-retry-client', project: 'AwsDotnetRetryClient', sampleName: 'AWS .NET Retry Client',
    focus: 'building .NET workloads on AWS with the SDK client factory, IAM roles, region-aware configuration, bounded resilience, observability, and service-specific operational design',
    boundary: 'The AWS SDK handles signing, serialization, endpoints, and service clients; the workload still owns IAM scope, deadlines, idempotency, retry policy, data classification, and recovery.',
    failure: 'Stacking application retries over SDK retries can amplify throttling, exceed user deadlines, duplicate side effects, and hide the dependency that is actually saturated.',
    extras: ['IAM role credentials', 'SDK retry and idempotency policy'], practices: ['Use IAM roles or workload identity and the default credential chain; never embed long-lived access keys in source or application settings.', 'Classify AWS errors by retry safety, honor service guidance and retry-after signals, cap attempts within the request deadline, and make writes idempotent.'],
    questions: [['How should a .NET app authenticate to AWS?', 'Use short-lived role credentials through the standard credential provider chain, with least-privilege IAM and no hard-coded keys.'], ['Why can default SDK retries be dangerous?', 'They are useful but invisible stacking with application retries can multiply calls and latency; configure one bounded retry budget using workload evidence.']],
    sampleDescription: 'A .NET 10 AWS-style client policy lab that classifies throttling and server faults, applies capped jittered backoff, and refuses unsafe retries.',
    code: `var failures = new[] { new CloudFailure("Throttling", true), new("TimeoutBeforeSend", true), new("Validation", false) };
var random = new Random(42);
foreach (var failure in failures)
{
    var delays = Enumerable.Range(1, 3).Where(_ => failure.Retryable).Select(attempt => Math.Min(1000, 100 * (1 << attempt)) + random.Next(0, 50));
    Console.WriteLine($"{failure.Code}: [{string.Join(',', delays)}]");
}
if (args.Contains("--self-test") && failures.Single(x => x.Code == "Validation").Retryable) return 1;
return 0;
sealed record CloudFailure(string Code, bool Retryable);`
  },
  {
    batch: 21, slug: 'azure-well-architected', sampleId: 'azure-waf-assessment', project: 'AzureWafAssessment', sampleName: 'Azure Well-Architected Assessment',
    focus: 'evaluating Azure workload decisions across reliability, security, cost optimization, operational excellence, and performance efficiency as connected trade-offs',
    boundary: 'The Well-Architected Framework guides workload decisions and reviews; Azure service compliance or a one-time assessment does not prove that a workload is well architected.',
    failure: 'Optimizing one pillar in isolation can shift unacceptable risk to another—for example, reducing redundancy may save cost while violating recovery objectives.',
    extras: ['Five-pillar trade-offs', 'Continuous workload review'], practices: ['Review a named workload and its critical user flows, not an abstract subscription or the whole organization at once.', 'Turn findings into owned, risk-ranked work with target dates, acceptance evidence, and an explicitly accepted residual risk.'],
    questions: [['What is the unit of a Well-Architected review?', 'A defined workload with business context, critical flows, architecture, operating model, and measurable requirements.'], ['How should pillar conflicts be resolved?', 'Use business criticality, risk appetite, constraints, and measurable scenarios to make and record an explicit trade-off.']],
    sampleDescription: 'A .NET 10 five-pillar workload assessment that weights findings by critical flow, impact, likelihood, evidence, owner, and remediation status.',
    code: `var findings = new[]
{
    new Finding("Reliability", "Checkout", 5, 4, false), new("Security", "Administration", 5, 3, true),
    new("Cost", "Reporting", 2, 3, false), new("Operations", "All", 4, 4, false), new("Performance", "Search", 3, 4, true)
};
foreach (var item in findings.OrderByDescending(x => x.Impact * x.Likelihood))
    Console.WriteLine($"{item.Pillar}/{item.Flow}: risk={item.Impact * item.Likelihood} evidence={item.HasEvidence}");
if (args.Contains("--self-test") && findings.Select(x => x.Pillar).Distinct().Count() != 5) return 1;
return 0;
sealed record Finding(string Pillar, string Flow, int Impact, int Likelihood, bool HasEvidence);`
  },
  {
    batch: 21, slug: 'azure-waf-reliability', sampleId: 'waf-reliability-budget', project: 'WafReliabilityBudget', sampleName: 'WAF Reliability Budget',
    focus: 'defining reliability requirements, failure modes, redundancy, recovery, graceful degradation, and operational learning for critical Azure workload flows',
    boundary: 'Reliability is an end-to-end workload property; choosing an availability-zone-capable service does not create a reliable user journey by itself.',
    failure: 'Undefined SLOs and recovery objectives lead teams to buy redundancy without knowing whether failover, data recovery, dependencies, and operations meet the business need.',
    extras: ['SLO and error budgets', 'Failure-mode and recovery design'], practices: ['Define SLIs and SLOs for critical flows, calculate error budgets, and connect burn-rate alerts to an owned response.', 'Set RTO, RPO, maximum tolerable outage, and data-consistency expectations; then prove them with restore and failover exercises.'],
    questions: [['What is an error budget?', 'The allowed unreliability implied by an SLO; it provides a shared quantitative limit for release risk and reliability work.'], ['Why is redundancy insufficient?', 'Dependencies, state replication, routing, health detection, failover logic, capacity, and operator action must all work within the recovery target.']],
    sampleDescription: 'A .NET 10 SLO calculator that derives monthly error budget, measures consumption, and raises fast- and slow-burn alerts.',
    code: `var target = 99.95;
var windowMinutes = 30d * 24 * 60;
var budgetMinutes = windowMinutes * (100 - target) / 100;
var incidents = new[] { new Incident("checkout", 7.5), new("checkout", 4.0) };
var consumed = incidents.Sum(x => x.Minutes);
Console.WriteLine($"SLO={target}% budget={budgetMinutes:F1}m consumed={consumed:F1}m remaining={budgetMinutes - consumed:F1}m");
if (args.Contains("--self-test") && (consumed >= budgetMinutes || budgetMinutes <= 0)) return 1;
return 0;
sealed record Incident(string Flow, double Minutes);`
  },
  {
    batch: 21, slug: 'azure-waf-security', sampleId: 'waf-security-threat-review', project: 'WafSecurityThreatReview', sampleName: 'WAF Security Threat Review',
    focus: 'protecting Azure workloads through zero trust, identity, data classification, network controls, secure delivery, detection, response, and continuous posture management',
    boundary: 'Cloud platform controls provide capabilities and signals; the workload team remains accountable for configuration, authorization, data use, code, dependencies, monitoring, and response.',
    failure: 'Perimeter-only security leaves workload identities, control planes, software supply chain, data flows, and privileged operations exposed to misuse.',
    extras: ['Zero-trust workload design', 'Threat modeling and incident response'], practices: ['Map identities, data, trust boundaries, entry points, administrative paths, threats, mitigations, detection, and residual risk for critical flows.', 'Prefer managed identities, least-privilege RBAC, just-in-time administration, private access where justified, and policy-backed secure defaults.'],
    questions: [['What does zero trust mean for a workload?', 'Explicitly verify identity, authorize least privilege, assume breach, protect data, and continuously use context and telemetry at every boundary.'], ['Why combine prevention and detection?', 'No preventive control is perfect; actionable detection and rehearsed response reduce impact when credentials, configuration, or code are compromised.']],
    sampleDescription: 'A .NET 10 threat-review model that maps assets, actors, trust crossings, preventive controls, detection, owner, and residual risk.',
    code: `var threats = new[]
{
    new Threat("Spoof workload identity", "Order data", true, true, 4),
    new("Exfiltrate logs", "Customer data", true, true, 3),
    new("Abuse admin action", "Control plane", true, false, 5)
};
foreach (var threat in threats.OrderByDescending(x => x.ResidualRisk))
    Console.WriteLine($"risk={threat.ResidualRisk} {threat.Name} prevent={threat.Prevent} detect={threat.Detect}");
var criticalUndetected = threats.Any(x => x.ResidualRisk >= 5 && !x.Detect);
if (args.Contains("--self-test") && !criticalUndetected) return 1;
return 0;
sealed record Threat(string Name, string Asset, bool Prevent, bool Detect, int ResidualRisk);`
  },
  {
    batch: 22, slug: 'azure-waf-cost-optimization', sampleId: 'waf-cost-unit-economics', project: 'WafCostUnitEconomics', sampleName: 'WAF Cost Unit Economics',
    focus: 'aligning Azure workload cost with business value using ownership, allocation, unit economics, demand shaping, rate optimization, waste removal, and financial guardrails',
    boundary: 'Cost optimization maximizes value for required quality levels; it is not indiscriminate spend reduction or a finance-only responsibility.',
    failure: 'Monthly totals without workload, environment, owner, and business-unit context hide idle waste, scaling inefficiency, regressions, and expensive architecture choices.',
    extras: ['Unit economics', 'FinOps feedback loops'], practices: ['Tag and allocate spend to workload, environment, owner, and business purpose, then track cost per business unit such as order or active tenant.', 'Set budgets and anomaly detection, right-size from utilization and demand evidence, and revalidate reservations or savings commitments as workloads change.'],
    questions: [['Why use unit cost?', 'It separates growth-driven spend from efficiency regression by relating cost to a stable business outcome such as transactions or customers.'], ['When are reservations appropriate?', 'For a measured stable baseline after architecture and utilization are understood; variable demand should keep suitable elasticity.']],
    sampleDescription: 'A .NET 10 unit-economics calculator that allocates shared Azure cost, calculates cost per order, detects idle spend, and flags budget variance.',
    code: `var costs = new[] { new Cost("Compute", 4200, .8), new("Database", 3100, .95), new("Observability", 900, .7), new("Idle dev", 650, 0) };
var orders = 125_000;
var total = costs.Sum(x => x.Amount);
var unit = total / orders;
var waste = costs.Where(x => x.Utilization < .1).Sum(x => x.Amount);
Console.WriteLine($"total={total:C0} cost/order={unit:C4} idle={waste:C0}");
if (args.Contains("--self-test") && (unit <= 0 || waste != 650)) return 1;
return 0;
sealed record Cost(string Meter, decimal Amount, double Utilization);`
  },
  {
    batch: 22, slug: 'azure-waf-operational-excellence', sampleId: 'waf-operations-release-readiness', project: 'WafOperationsReleaseReadiness', sampleName: 'WAF Operations Release Readiness',
    focus: 'operating Azure workloads through observable standards, safe automation, deployment discipline, actionable alerts, runbooks, learning reviews, and continuous improvement',
    boundary: 'Operational excellence is designed into the workload and delivery system; an operations team cannot add it after development through dashboards alone.',
    failure: 'Manual, undocumented, high-privilege changes create configuration drift, slow recovery, inconsistent environments, and incidents whose cause cannot be reconstructed.',
    extras: ['Safe deployment practices', 'Operational readiness and learning'], practices: ['Build immutable artifacts once, provision and configure through reviewed automation, and use staged exposure with health-based rollback.', 'Make alerts actionable with user impact, severity, owner, evidence, and runbook; delete alerts that cannot drive a response.'],
    questions: [['What is operational readiness?', 'Evidence that ownership, telemetry, alerts, runbooks, capacity, security, recovery, deployment, support, and known risks are ready before launch.'], ['Why prefer small reversible changes?', 'They reduce blast radius, simplify diagnosis, shorten feedback, and make rollback safer when signals deteriorate.']],
    sampleDescription: 'A .NET 10 release-readiness gate that verifies ownership, SLOs, dashboards, alerts, runbooks, rollback, capacity, security, and recovery evidence.',
    code: `var gates = new[]
{
    new Gate("Owner and on-call", true), new("SLO dashboard", true), new("Actionable alerts", true),
    new("Rollback rehearsed", true), new("Restore evidence", false), new("Capacity evidence", true)
};
foreach (var gate in gates) Console.WriteLine($"{(gate.Ready ? "PASS" : "BLOCK")} {gate.Name}");
var ready = gates.All(x => x.Ready);
if (args.Contains("--self-test") && ready) return 1;
return 0;
sealed record Gate(string Name, bool Ready);`
  },
  {
    batch: 22, slug: 'azure-waf-performance-efficiency', sampleId: 'waf-performance-capacity', project: 'WafPerformanceCapacity', sampleName: 'WAF Performance Capacity',
    focus: 'meeting workload latency and throughput targets efficiently through demand modeling, architecture, scaling, caching, data design, testing, and continuous measurement',
    boundary: 'Performance efficiency is an end-to-end workload property; scaling one Azure resource cannot fix chatty calls, poor queries, hot partitions, retry storms, or downstream limits.',
    failure: 'Average latency and CPU alone hide tail latency, queueing, saturation, hot keys, connection limits, dependency throttling, and failure-time capacity collapse.',
    extras: ['Capacity and demand modeling', 'Tail latency and saturation'], practices: ['Define performance scenarios with workload shape, concurrency, payload, data volume, p95 or p99 latency, throughput, and dependency conditions.', 'Load-test steady, peak, spike, soak, failover, and recovery behavior; validate autoscale lead time and the capacity of every dependency.'],
    questions: [['Why track tail latency?', 'Averages hide the slow requests users experience; p95 and p99 expose queueing, outliers, and dependency variability.'], ['What does Little’s Law provide?', 'For a stable system, concurrency is approximately throughput multiplied by time in system, useful for checking capacity assumptions.']],
    sampleDescription: 'A .NET 10 capacity planner using Little’s Law, headroom, autoscale lead time, and per-instance throughput to estimate safe instance count.',
    code: `var arrivalPerSecond = 750d;
var p95Seconds = .18;
var observedConcurrency = arrivalPerSecond * p95Seconds;
var perInstance = 110d;
var headroom = 1.35;
var instances = (int)Math.Ceiling(arrivalPerSecond * headroom / perInstance);
Console.WriteLine($"concurrency≈{observedConcurrency:F0} instances={instances} headroom={headroom:P0}");
if (args.Contains("--self-test") && (instances < 10 || observedConcurrency <= 0)) return 1;
return 0;`
  },
  {
    batch: 23, slug: 'yarp-reverse-proxy', sampleId: 'yarp-route-policy-lab', project: 'YarpRoutePolicyLab', sampleName: 'YARP Route Policy Lab',
    focus: 'building a programmable .NET reverse proxy with explicit routes, clusters, transforms, health, load balancing, resilience, authentication boundaries, and dynamic configuration',
    boundary: 'YARP proxies HTTP traffic and exposes extensibility; upstream and downstream services still own business authorization, data rules, contracts, and workload-specific resilience.',
    failure: 'A catch-all proxy can become an unbounded trust bridge that forwards spoofed headers, retries unsafe requests, hides unhealthy destinations, or centralizes fragile business logic.',
    extras: ['Routes, clusters, and transforms', 'Destination health and proxy resilience'], practices: ['Match routes narrowly by host, path, method, and policy; reject ambiguous ownership and strip client-supplied forwarding or identity headers.', 'Use active and passive health, bounded timeouts, safe retry policy, destination capacity, and graceful configuration changes based on measured traffic.'],
    questions: [['What is the difference between a YARP route and cluster?', 'A route matches an incoming request and selects policy; a cluster groups destination endpoints and load-balancing or health behavior.'], ['Where should authorization occur?', 'The edge can enforce coarse policy, but the destination must authorize the operation and resource because it owns the business trust decision.']],
    sampleDescription: 'A .NET 10 YARP policy model that performs ordered route matching, strips spoofable headers, selects healthy destinations, and rejects unsafe retries.',
    code: `var routes = new[] { new Route("orders", "/api/orders", new[] { "GET", "POST" }, "orders"), new("catalog", "/api/catalog", new[] { "GET" }, "catalog") };
var destinations = new[] { new Destination("orders-a", "orders", true, 2), new("orders-b", "orders", false, 0), new("catalog-a", "catalog", true, 1) };
var request = new Request("POST", "/api/orders/42", new Dictionary<string, string> { ["X-Forwarded-User"] = "spoofed" });
request.Headers.Remove("X-Forwarded-User");
var route = routes.First(x => request.Path.StartsWith(x.Prefix) && x.Methods.Contains(request.Method));
var target = destinations.Where(x => x.Cluster == route.Cluster && x.Healthy).OrderBy(x => x.InFlight).First();
Console.WriteLine($"route={route.Name} target={target.Name} headers={request.Headers.Count}");
if (args.Contains("--self-test") && (target.Name != "orders-a" || request.Headers.Count != 0)) return 1;
return 0;
sealed record Route(string Name, string Prefix, string[] Methods, string Cluster);
sealed record Destination(string Name, string Cluster, bool Healthy, int InFlight);
sealed record Request(string Method, string Path, Dictionary<string, string> Headers);`
  },
  {
    batch: 23, slug: 'structured-logging', sampleId: 'structured-logging-correlation', project: 'StructuredLoggingCorrelation', sampleName: 'Structured Logging Correlation',
    focus: 'emitting stable event templates and typed properties that correlate requests, traces, dependencies, deployments, tenants, and business outcomes without leaking sensitive data',
    boundary: 'Structured logs explain discrete events; metrics quantify trends and alerts, traces connect distributed work, and audits provide tamper-aware accountability.',
    failure: 'Interpolated text, unbounded property values, inconsistent names, duplicate exception logging, or sensitive payloads make logs expensive, unqueryable, and dangerous.',
    extras: ['Event schema and correlation', 'Redaction, cardinality, and retention'], practices: ['Define stable event names, templates, severity, typed properties, ownership, and retention; keep high-cardinality values out of metric dimensions.', 'Log identifiers and outcomes rather than payloads, tokens, secrets, or personal data; test redaction and access controls as part of delivery.'],
    questions: [['Why use message templates?', 'They preserve an event shape and typed properties for reliable querying while rendering readable text.'], ['How do logs differ from audit records?', 'Logs support diagnosis; audit records need stronger completeness, access, integrity, retention, and actor/action/resource semantics.']],
    sampleDescription: 'A .NET 10 JSON log emitter with stable event names, trace correlation, typed properties, exception classification, and allowlist-based redaction.',
    code: `using System.Text.Json;

var traceId = Guid.NewGuid().ToString("N");
var input = new Dictionary<string, object?> { ["order.id"] = "ORD-42", ["customer.email"] = "person@example.com", ["duration.ms"] = 37 };
var allowed = new HashSet<string> { "order.id", "duration.ms" };
var properties = input.ToDictionary(x => x.Key, x => allowed.Contains(x.Key) ? x.Value : "[REDACTED]");
var entry = new { timestamp = DateTimeOffset.UtcNow, level = "Information", eventName = "OrderAccepted", traceId, properties };
Console.WriteLine(JsonSerializer.Serialize(entry));
if (args.Contains("--self-test") && !Equals(properties["customer.email"], "[REDACTED]")) return 1;
return 0;`
  },
  {
    batch: 23, slug: 'interview-questions', sampleId: 'dotnet-interview-practice', project: 'DotnetInterviewPractice', sampleName: '.NET Interview Practice',
    focus: 'preparing evidence-based .NET architecture answers that explain context, trade-offs, implementation, failure handling, verification, and measured outcomes',
    boundary: 'Interview preparation organizes knowledge and practice; strong answers remain honest about personal experience, uncertainty, alternatives, and the actual result.',
    failure: 'Memorized definitions without context, trade-offs, failure modes, or evidence sound shallow and collapse under follow-up questions.',
    extras: ['Answer structure and trade-offs', 'Deliberate practice and evidence'], practices: ['Answer architecture questions with context, constraints, options, decision, implementation, failure handling, evidence, and what you would improve.', 'Keep a story bank mapped to competencies, quantify outcomes honestly, and practice progressively deeper follow-up questions.'],
    questions: [['How should you answer a system-design question?', 'Clarify requirements and qualities, estimate scale, define boundaries and data, explain trade-offs, cover failure/security/operations, and state how you would validate.'], ['What should you do when you do not know?', 'Say what you know, name the uncertainty, reason from fundamentals, propose how you would verify, and avoid inventing experience.']],
    sampleDescription: 'A .NET 10 interview-practice engine that schedules prompts, scores answers against a transparent architecture rubric, and prioritizes weak competencies.',
    code: `var prompts = new[]
{
    new Prompt("Design a reliable checkout API", new[] { "context", "trade-off", "failure", "evidence" }),
    new("Explain idempotency", new[] { "definition", "example", "failure", "verification" })
};
var answer = "context trade-off failure evidence rollback measurement";
foreach (var prompt in prompts)
{
    var score = prompt.Criteria.Count(c => answer.Contains(c, StringComparison.OrdinalIgnoreCase));
    Console.WriteLine($"{score}/{prompt.Criteria.Length} {prompt.Question}");
}
if (args.Contains("--self-test") && prompts[0].Criteria.Any(c => !answer.Contains(c))) return 1;
return 0;
sealed record Prompt(string Question, string[] Criteria);`
  }
];

const bySlug = new Map(specs.map(spec => [spec.slug, spec]));

function replaceTopLevelObjects(source, records, key) {
  const replacements = new Map(records.map(record => [record[key], record]));
  const spans = [];
  let depth = 0;
  let start = -1;
  let inString = false;
  let escaped = false;
  for (let index = 0; index < source.length; index++) {
    const character = source[index];
    if (inString) {
      if (escaped) escaped = false;
      else if (character === '\\') escaped = true;
      else if (character === '"') inString = false;
      continue;
    }
    if (character === '"') { inString = true; continue; }
    if (character === '{') {
      depth++;
      if (depth === 1) start = index;
    } else if (character === '}') {
      if (depth === 1 && start >= 0) spans.push([start, index + 1]);
      depth--;
    }
  }

  let output = source;
  const found = new Set();
  for (const [from, to] of spans.reverse()) {
    const parsed = JSON.parse(source.slice(from, to));
    const replacement = replacements.get(parsed[key]);
    if (!replacement) continue;
    found.add(parsed[key]);
    const formatted = JSON.stringify(replacement, null, 2).replaceAll('\n', '\n  ');
    output = `${output.slice(0, from)}${formatted}${output.slice(to)}`;
  }

  const missing = records.filter(record => !found.has(record[key]));
  if (missing.length) {
    const closing = output.lastIndexOf(']');
    const before = output.slice(0, closing).trimEnd();
    const separator = before.endsWith('[') ? '\n' : ',\n';
    const additions = missing.map(record => `  ${JSON.stringify(record, null, 2).replaceAll('\n', '\n  ')}`).join(',\n');
    output = `${before}${separator}${additions}\n${output.slice(closing)}`;
  }
  return output;
}

function makeDeepTopic(topic, spec) {
  const overviewBase = topic.overview.split(' This guide connects the programming model')[0];
  const detailBase = topic.detail.split(' The central design goal is ')[0];
  const existingConcepts = topic.concepts ?? [];
  const concepts = [...existingConcepts];
  for (const title of spec.extras) {
    if (!concepts.some(item => item.title === title)) concepts.push({ title, description: `${title} is a first-class design concern for ${spec.focus}.` });
  }
  while (concepts.length < 6) {
    concepts.push({ title: `Operational boundary ${concepts.length + 1}`, description: `${spec.boundary} This boundary must be explicit in design, code, telemetry, and ownership.` });
  }
  const bestPractices = [...new Set([...(topic.bestPractices ?? []), ...spec.practices])].slice(0, 8);
  const interviewQuestions = [...new Map([...(topic.interviewQuestions ?? []), ...spec.questions.map(([question, answer]) => ({ question, answer }))].map(item => [item.question, item])).values()];
  if (interviewQuestions.length < 5) {
    interviewQuestions.push({ question: `How would you introduce ${topic.name} safely?`, answer: 'Start with one measurable user journey, make ownership and trust boundaries explicit, test the highest-risk assumption, release gradually, and compare operational evidence to the acceptance criteria.' });
  }
  return {
    ...topic,
    contentStatus: 'complete',
    overview: `${overviewBase} This guide connects the programming model to production decisions, failure modes, security, testing, performance, deployment, and operations.`,
    detail: `${detailBase} The central design goal is ${spec.focus}. ${spec.boundary} Teams should begin with a concrete user outcome and quality-attribute scenario, choose the smallest architecture that satisfies it, and record the operational owner. ${spec.failure} A production design therefore needs explicit limits, identity, cancellation, telemetry, evaluation, recovery, and a safe change path. The dedicated sample keeps external infrastructure optional so readers can inspect the governing mechanics locally, then maps those mechanics to the hosted services and controls required in a real environment.`,
    learningObjectives: [
      `Explain the purpose and boundaries of ${topic.name} without confusing the tool with the whole architecture.`,
      `Decide when ${topic.name} is appropriate and when a simpler design is safer.`,
      `Design the core contracts, ownership, data flow, and failure behavior for ${spec.focus}.`,
      'Apply authentication, authorization, validation, privacy, and least privilege at every trust boundary.',
      'Test normal behavior, negative paths, dependency failure, recovery, and operational signals.',
      'Deploy incrementally with observable acceptance criteria, rollback, and accountable ownership.',
      `Run and extend the dedicated ${spec.sampleName} sample.`
    ],
    prerequisites: [
      '.NET 10, C# async programming, dependency injection, configuration, and structured logging.',
      `The domain and platform fundamentals referenced by ${topic.name}.`,
      'Basic security, testing, observability, and continuous-delivery concepts.',
      `A .NET 10 SDK to run ${spec.sampleName}; no paid cloud account is required for its default path.`
    ],
    decisionGuide: {
      use: [`Use ${topic.name} when ${spec.focus} directly addresses a measured product or operational need.`, 'Use it when a named team can own the contracts, data, deployment, telemetry, cost, and incident response.', 'Introduce it behind a narrow boundary and prove the riskiest assumption with a production-shaped experiment.'],
      avoid: [`Avoid ${topic.name} when the same outcome is available through a smaller in-process or managed capability.`, 'Do not adopt it only for résumé value, diagram symmetry, or a vendor checklist.', 'Do not proceed without a threat model, failure policy, observability, and an exit or rollback path.'],
      tradeoffs: [spec.boundary, spec.failure, 'More abstraction can improve consistency and portability while hiding provider-specific behavior that still needs measurement.', 'Automation reduces repeated work but increases the need for policy, audit, ownership, and safe defaults.']
    },
    concepts: concepts.slice(0, 8),
    implementationSteps: [
      'Write the user outcome, quality attributes, constraints, trust boundaries, and measurable acceptance criteria.',
      'Define stable application contracts and keep provider or platform details at the composition boundary.',
      'Implement the smallest end-to-end path with bounded input, cancellation, explicit errors, and deterministic local behavior.',
      'Add identity, least privilege, privacy controls, approval for consequential actions, and auditable operation identifiers.',
      'Instrument latency, errors, saturation, cost, dependency behavior, and the business outcome using correlated telemetry.',
      'Exercise negative paths, duplicates, timeouts, invalid input, partial failure, recovery, and compatibility changes.',
      'Release gradually, compare signals to acceptance criteria, document rollback, and assign lifecycle ownership.'
    ],
    testing: { introduction: `Test ${topic.name} as a behavior and operational boundary, not only as a library call.`, items: ['Unit-test deterministic policy, validation, mapping, and error classification.', 'Use contract tests for provider, protocol, platform, and downstream boundaries.', 'Run integration tests with realistic serialization, configuration, identity, cancellation, and dependency behavior.', `Exercise the primary failure: ${spec.failure}`, 'Load-test representative and adversarial inputs while observing latency, errors, saturation, and cost.', 'Keep a versioned acceptance suite that protects behavior during dependency and configuration changes.'] },
    security: { introduction: `Treat ${topic.name} input, dependencies, credentials, output, and administrative controls as separate trust boundaries.`, items: ['Authenticate the caller and workload; authorize the operation and resource at the system that performs it.', 'Validate type, size, count, format, tenant, and allowed values before expensive or consequential work.', 'Use short-lived identity and least privilege; keep secrets out of source, logs, prompts, packages, and client storage.', 'Classify data, minimize collection and retention, redact telemetry, encrypt in transit and at rest, and support deletion policy.', 'Require explicit approval and idempotency for external side effects; audit actor, intent, policy decision, and outcome.', 'Patch and inventory dependencies, pin trusted sources, scan artifacts, and restrict outbound destinations.'] },
    performance: { introduction: 'Optimize the end-to-end user outcome within downstream capacity and cost limits.', items: ['Measure p50, p95, p99, throughput, failures, saturation, queue time, payload size, and cost together.', 'Set end-to-end deadlines and propagate cancellation instead of stacking independent timeouts.', 'Bound concurrency, queues, retries, payloads, fan-out, and caches so overload cannot grow without limit.', 'Cache only when identity, freshness, privacy, invalidation, and failure semantics are explicit.', 'Batch or stream only when the workload and user latency target justify the complexity.', 'Capacity-test dependency degradation and recovery because retry and replay traffic often dominate peak load.'] },
    deployment: { introduction: 'Promote an immutable, observable change through environments with compatibility and rollback evidence.', items: ['Keep environment configuration external, validated at startup, and free of long-lived credentials.', 'Provision dependencies, identity, network policy, dashboards, alerts, and budgets through reviewed automation.', 'Use additive contracts and mixed-version compatibility before removing old behavior.', 'Deploy with canary or staged exposure, health gates, graceful shutdown, and automated rollback signals.', 'Record version and configuration in telemetry; retain runbooks, ownership, recovery targets, and escalation paths.', 'Rehearse dependency loss, restore, credential rotation, region or device failure, and data recovery where applicable.'] },
    troubleshooting: [
      { symptom: 'The local sample works but production behavior differs', cause: 'Provider, identity, network, limits, data, or configuration assumptions were hidden.', fix: 'Compare effective configuration and correlated dependency telemetry, then reproduce the smallest differing boundary.' },
      { symptom: 'Latency rises sharply under load', cause: 'Concurrency, fan-out, retries, queues, or a downstream quota is unbounded.', fix: 'Inspect saturation and queue time, stop retry amplification, apply backpressure, and reduce work per request.' },
      { symptom: 'A request succeeds twice or creates duplicate effects', cause: 'The caller retried after an ambiguous result and the operation was not idempotent.', fix: 'Use a stable operation ID, persist the outcome with the effect, and return the recorded result on replay.' },
      { symptom: 'Sensitive information appears in telemetry', cause: 'Raw inputs, outputs, headers, or exceptions were logged without classification and redaction.', fix: 'Stop the leak, restrict and rotate access as needed, redact by default, and test the telemetry schema.' },
      { symptom: 'A dependency upgrade changes behavior', cause: 'Application semantics depended on provider-specific defaults or an untested protocol detail.', fix: 'Pin or roll back, compare release notes and contract traces, then update the acceptance suite before retrying.' },
      { symptom: 'Operators cannot explain a failed user outcome', cause: 'Logs, metrics, traces, version, and business identifiers are not correlated.', fix: 'Add end-to-end correlation and a dashboard that links the user journey to dependency and deployment signals.' }
    ],
    productionChecklist: [
      `The need for ${topic.name}, alternatives, owner, constraints, and exit criteria are recorded.`,
      'Contracts, data ownership, trust boundaries, limits, and compatibility policy are explicit.',
      'Identity, least privilege, validation, privacy, redaction, audit, and side-effect approval are enforced.',
      'Timeout, cancellation, retry, backpressure, idempotency, fallback, and recovery behavior are tested.',
      'Logs, metrics, traces, cost, quality, business outcomes, dashboards, alerts, and runbooks identify version and owner.',
      'Capacity, dependency failure, restore, rollback, credential rotation, and incident response are rehearsed.',
      'Lifecycle, deprecation, data retention, dependency patching, and operational cost have named owners.'
    ],
    bestPractices,
    interviewQuestions: interviewQuestions.slice(0, 6),
    note: `${spec.boundary}`,
    tip: `Start with the dedicated ${spec.sampleName} sample, then replace one local boundary at a time with production infrastructure.`,
    warning: spec.failure,
    sampleId: spec.sampleId,
    readingMinutes: Math.max(24, topic.readingMinutes ?? 0),
    level: topic.level ?? 'Intermediate',
    updated: today,
    latestNote: `Complete production guide with dedicated ${spec.sampleName} sample`
  };
}

function sampleRecord(spec) {
  return {
    id: spec.sampleId, topicSlug: spec.slug, name: spec.sampleName, description: spec.sampleDescription,
    technologies: ['.NET 10', spec.sampleName], tags: ['dedicated-sample', `batch-${spec.batch}`, spec.slug],
    runCommand: `dotnet run --project src/${spec.project}`,
    testCommand: `dotnet run --project src/${spec.project} -- --self-test`,
    status: 'complete',
    githubUrl: `https://github.com/adhakshinamoorthy/adhakshinamoorthy.github.io/tree/main/samples/${spec.sampleId}`,
    localPath: `samples/${spec.sampleId}/README.md`, liveDemoUrl: null
  };
}

async function writeSample(spec) {
  const sampleRoot = join(root, 'samples', spec.sampleId);
  const sourceRoot = join(sampleRoot, 'src', spec.project);
  await mkdir(sourceRoot, { recursive: true });
  await writeFile(join(sampleRoot, 'Directory.Build.props'), '<Project><PropertyGroup><Nullable>enable</Nullable><ImplicitUsings>enable</ImplicitUsings><TreatWarningsAsErrors>true</TreatWarningsAsErrors><LangVersion>latest</LangVersion></PropertyGroup></Project>\n');
  await writeFile(join(sampleRoot, `${spec.project}.slnx`), `<Solution><Project Path="src/${spec.project}/${spec.project}.csproj" /></Solution>\n`);
  await writeFile(join(sourceRoot, `${spec.project}.csproj`), '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup><OutputType>Exe</OutputType><TargetFramework>net10.0</TargetFramework></PropertyGroup></Project>\n');
  await writeFile(join(sourceRoot, 'Program.cs'), `${spec.code}\n`);
  await writeFile(join(sampleRoot, 'README.md'), `# ${spec.sampleName}\n\n${spec.sampleDescription}\n\n## What it demonstrates\n\n- ${spec.focus}.\n- ${spec.boundary}\n- A credential-free local path with deterministic output and a small self-check.\n\n## Run\n\n\`\`\`powershell\n${sampleRecord(spec).runCommand}\n\`\`\`\n\n## Check\n\n\`\`\`powershell\n${sampleRecord(spec).testCommand}\n\`\`\`\n\n## Production boundary\n\n${spec.failure} Replace local stores and fake dependencies deliberately, retain the validation and policy boundaries, add authenticated workload identity, durable state where required, correlated telemetry, capacity limits, and an operator runbook.\n`);
}

const technologiesPath = join(root, 'data', 'technologies.json');
const samplesPath = join(root, 'data', 'github-samples.json');
const technologies = JSON.parse(await readFile(technologiesPath, 'utf8'));
const samples = JSON.parse(await readFile(samplesPath, 'utf8'));

for (let index = 0; index < technologies.length; index++) {
  const spec = bySlug.get(technologies[index].slug);
  if (spec) technologies[index] = makeDeepTopic(technologies[index], spec);
}
for (const spec of specs) {
  const index = samples.findIndex(sample => sample.id === spec.sampleId);
  const record = sampleRecord(spec);
  if (index >= 0) samples[index] = record; else samples.push(record);
  await writeSample(spec);
}

if (process.argv.includes('--from-head')) {
  const safeRoot = root.replaceAll('\\', '/');
  const git = file => execFileSync('git', ['-c', `safe.directory=${safeRoot}`, 'show', `HEAD:${file}`], { cwd: root, encoding: 'utf8' });
  const topicRecords = technologies.filter(topic => bySlug.has(topic.slug));
  const sampleRecords = specs.map(spec => sampleRecord(spec));
  await writeFile(technologiesPath, replaceTopLevelObjects(git('data/technologies.json'), topicRecords, 'slug'));
  await writeFile(samplesPath, replaceTopLevelObjects(git('data/github-samples.json'), sampleRecords, 'id'));
} else {
  await writeFile(technologiesPath, replaceTopLevelObjects(await readFile(technologiesPath, 'utf8'), technologies.filter(topic => bySlug.has(topic.slug)), 'slug'));
  await writeFile(samplesPath, replaceTopLevelObjects(await readFile(samplesPath, 'utf8'), specs.map(spec => sampleRecord(spec)), 'id'));
}
console.log(`Completed ${specs.length} topics and dedicated samples through Batch ${Math.max(...specs.map(spec => spec.batch))}.`);
