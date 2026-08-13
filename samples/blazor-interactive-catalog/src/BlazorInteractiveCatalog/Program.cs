using BlazorInteractiveCatalog.Components;
using BlazorInteractiveCatalog.Services;
using Microsoft.AspNetCore.DataProtection;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddSimpleConsole(options => options.TimestampFormat = "HH:mm:ss ");
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDataProtection().UseEphemeralDataProtectionProvider();
}

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSingleton<ProductCatalog>();
builder.Services.AddScoped<CartState>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next(context);
});
app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

public partial class Program;
