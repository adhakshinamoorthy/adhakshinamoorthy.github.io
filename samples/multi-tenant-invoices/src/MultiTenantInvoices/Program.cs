using Microsoft.AspNetCore.Authentication;
using MultiTenantInvoices;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("LocalTenant").AddScheme<AuthenticationSchemeOptions, LocalTenantAuthenticationHandler>("LocalTenant", _ => { });
builder.Services.AddAuthorizationBuilder().SetFallbackPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build());
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<TenantInvoiceRepository>();
builder.Services.AddSingleton<InvoiceStore>();
builder.Services.AddSingleton<TenantQuota>();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<TenantContextMiddleware>();

var invoices = app.MapGroup("/api/invoices");
invoices.MapGet("/", (TenantInvoiceRepository repository) => Results.Ok(repository.List()));
invoices.MapGet("/{id:guid}", (Guid id, TenantInvoiceRepository repository) => repository.Find(id) is { } invoice ? Results.Ok(invoice) : Results.NotFound());
invoices.MapPost("/", (CreateInvoiceRequest request, TenantInvoiceRepository repository, TenantContext tenant, TenantQuota quota) =>
{
    if (string.IsNullOrWhiteSpace(request.Number) || request.Number.Length > 40 || request.Amount is <= 0 or > 1_000_000)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["invoice"] = ["Number and amount are outside allowed bounds."] });
    if (!quota.TryConsume(tenant.TenantId.Value)) return Results.StatusCode(StatusCodes.Status429TooManyRequests);
    var invoice = repository.Add(request);
    return Results.Created($"/api/invoices/{invoice.Id}", invoice);
});
app.Run();

public partial class Program;
