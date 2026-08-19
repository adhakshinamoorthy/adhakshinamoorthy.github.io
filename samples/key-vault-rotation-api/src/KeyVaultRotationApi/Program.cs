using KeyVaultRotationApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<InMemoryVersionedSecretStore>();
builder.Services.AddSingleton<IVersionedSecretStore>(services => services.GetRequiredService<InMemoryVersionedSecretStore>());
builder.Services.AddSingleton<RotatingSecretCache>();
var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ready" }));
app.MapPost("/admin/secrets/{name}/versions", (string name, SecretWrite request, InMemoryVersionedSecretStore store) => Results.Accepted(value: store.Set(name, request.Value)));
app.MapPost("/admin/secrets/{name}/activate/{version}", (string name, string version, InMemoryVersionedSecretStore store) => store.Activate(name, version) ? Results.NoContent() : Results.NotFound());
app.MapGet("/payments/credential", (RotatingSecretCache cache) => { var secret = cache.Get("Payments--ApiKey"); return Results.Ok(new { secret.Name, secret.Version, fingerprint = secret.Fingerprint }); });

app.Run();

public sealed record SecretWrite(string Value);
