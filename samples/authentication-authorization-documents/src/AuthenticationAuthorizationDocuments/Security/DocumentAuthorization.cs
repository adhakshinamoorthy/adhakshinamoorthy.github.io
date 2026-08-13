using System.Security.Claims;
using AuthenticationAuthorizationDocuments.Documents;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace AuthenticationAuthorizationDocuments.Security;

internal static class Policies
{
    public const string WriteDocuments = "documents.write";
}

internal static class DocumentOperations
{
    public static readonly OperationAuthorizationRequirement Read = new() { Name = nameof(Read) };
}

internal sealed class DocumentOwnerHandler
    : AuthorizationHandler<OperationAuthorizationRequirement, Document>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OperationAuthorizationRequirement requirement,
        Document resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (requirement.Name == DocumentOperations.Read.Name &&
            (userId == resource.OwnerId || context.User.IsInRole("admin")))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
