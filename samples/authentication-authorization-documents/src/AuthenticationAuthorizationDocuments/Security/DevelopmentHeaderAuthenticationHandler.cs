using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace AuthenticationAuthorizationDocuments.Security;

internal sealed class DevelopmentHeaderAuthenticationOptions : AuthenticationSchemeOptions;

internal sealed class DevelopmentHeaderAuthenticationHandler(
    IOptionsMonitor<DevelopmentHeaderAuthenticationOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<DevelopmentHeaderAuthenticationOptions>(options, logger, encoder)
{
    public const string SchemeName = "DevelopmentHeaders";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-User"].ToString();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, userId) };
        foreach (var scope in Request.Headers["X-Scope"].ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            claims.Add(new Claim("scope", scope));
        }
        foreach (var role in Request.Headers["X-Role"].ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, SchemeName));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, SchemeName)));
    }
}
