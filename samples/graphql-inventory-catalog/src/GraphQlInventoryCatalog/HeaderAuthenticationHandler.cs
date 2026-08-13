using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GraphQlInventoryCatalog;

internal sealed class HeaderAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var id = Request.Headers["X-User-Id"].ToString();
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(AuthenticateResult.NoResult());
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, id) };
        var permission = Request.Headers["X-Permission"].ToString();
        if (!string.IsNullOrWhiteSpace(permission)) claims.Add(new Claim("permission", permission));
        var principal = new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name));
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
