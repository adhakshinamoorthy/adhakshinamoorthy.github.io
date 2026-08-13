using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace SignalROperationsRoom;

internal sealed class HeaderAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var id = Request.Headers["X-User-Id"].ToString();
        if (string.IsNullOrWhiteSpace(id)) return Task.FromResult(AuthenticateResult.NoResult());
        if (id.Length > 60) return Task.FromResult(AuthenticateResult.Fail("Invalid user identifier."));
        var identity = new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, id), new Claim(ClaimTypes.Name, id)], Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name)));
    }
}
