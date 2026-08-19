using ContainerAppsRevisionApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<ReadinessState>();
builder.Services.AddSingleton(_ => RevisionInfo.FromEnvironment());
builder.Services.AddHostedService<LifecycleService>();

var app = builder.Build();
app.MapGet("/", (RevisionInfo revision) => Results.Ok(new
{
    service = "container-apps-revision-api",
    revision.Name,
    revision.Replica,
    revision.Region
}));
app.MapGet("/health/live", () => Results.Ok(new { status = "live" }));
app.MapGet("/health/ready", (ReadinessState state) =>
    state.IsReady ? Results.Ok(new { status = "ready" }) : Results.StatusCode(503));
app.Run();

public partial class Program;
