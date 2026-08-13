using System.Security.Claims;
using AuthenticationAuthorizationDocuments.Documents;
using AuthenticationAuthorizationDocuments.Security;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddSingleton<IDocumentRepository, InMemoryDocumentRepository>();
builder.Services
    .AddAuthentication(DevelopmentHeaderAuthenticationHandler.SchemeName)
    .AddScheme<DevelopmentHeaderAuthenticationOptions, DevelopmentHeaderAuthenticationHandler>(
        DevelopmentHeaderAuthenticationHandler.SchemeName,
        _ => { });
builder.Services.AddSingleton<IAuthorizationHandler, DocumentOwnerHandler>();
builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build())
    .AddPolicy(Policies.WriteDocuments, policy => policy.RequireClaim("scope", Policies.WriteDocuments));

var app = builder.Build();
app.UseExceptionHandler();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => TypedResults.Ok(new { status = "healthy" }))
    .AllowAnonymous();

var documents = app.MapGroup("/api/documents").WithTags("Documents");

documents.MapGet("/{id:guid}", async Task<IResult> (
    Guid id,
    ClaimsPrincipal user,
    IDocumentRepository repository,
    IAuthorizationService authorization) =>
{
    var document = repository.Find(id);
    if (document is null)
    {
        return TypedResults.NotFound();
    }

    var result = await authorization.AuthorizeAsync(user, document, DocumentOperations.Read);
    return result.Succeeded
        ? TypedResults.Ok(DocumentResponse.From(document))
        : TypedResults.NotFound();
}).WithName("GetDocument");

documents.MapPost("/", (CreateDocumentRequest request, ClaimsPrincipal user, IDocumentRepository repository) =>
{
    var ownerId = user.FindFirstValue(ClaimTypes.NameIdentifier)!;
    var document = repository.Add(ownerId, request.Title, request.Content);
    return TypedResults.CreatedAtRoute(DocumentResponse.From(document), "GetDocument", new { id = document.Id });
}).RequireAuthorization(Policies.WriteDocuments);

app.Run();

public partial class Program;
