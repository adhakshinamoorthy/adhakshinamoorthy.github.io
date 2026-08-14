using Azure.Storage.Blobs;
using AzureDotNetServiceBaseline;

var builder = WebApplication.CreateBuilder(args);
var azureOptions = AzurePlatformOptions.FromConfiguration(builder.Configuration);
var credential = AzureCredentialFactory.Create(builder.Environment, azureOptions);

builder.Services.AddSingleton(azureOptions);
builder.Services.AddSingleton(credential);
builder.Services.AddSingleton(sp => new BlobServiceClient(
    sp.GetRequiredService<AzurePlatformOptions>().StorageEndpoint,
    sp.GetRequiredService<Azure.Core.TokenCredential>()));
builder.Services.AddHealthChecks();

var app = builder.Build();
app.MapGet("/", (AzurePlatformOptions options, IHostEnvironment environment) => Results.Ok(new
{
    service = "azure-dotnet-service-baseline",
    environment = environment.EnvironmentName,
    storageEndpoint = options.StorageEndpoint,
    authentication = environment.IsDevelopment() ? "developer-credential-chain" : "managed-identity"
}));
app.MapHealthChecks("/health/live");
app.Run();

public partial class Program;
