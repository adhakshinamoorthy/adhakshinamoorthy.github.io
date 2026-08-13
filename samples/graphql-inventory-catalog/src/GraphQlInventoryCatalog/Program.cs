using GraphQlInventoryCatalog;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("LocalHeader").AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>("LocalHeader", _ => { });
builder.Services.AddAuthorizationBuilder().AddPolicy("inventory.write", policy => policy.RequireClaim("permission", "inventory.write"));
builder.Services.AddSingleton<ProductStore>();
builder.Services.AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .ModifyRequestOptions(options => options.IncludeExceptionDetails = builder.Environment.IsDevelopment());

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapGraphQL();
app.MapGet("/", () => Results.Redirect("/graphql"));
app.Run();

public partial class Program;
