using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace MultiTenantInvoices;

internal sealed class LocalTenantAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers["X-User-Id"].ToString();
        var tenantId = Request.Headers["X-Tenant-Id"].ToString();
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(tenantId)) return Task.FromResult(AuthenticateResult.NoResult());
        if (!TenantId.TryCreate(tenantId, out var tenant)) return Task.FromResult(AuthenticateResult.Fail("Invalid tenant identifier."));
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, userId), new Claim("tenant_id", tenant.Value) };
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(new ClaimsIdentity(claims, Scheme.Name)), Scheme.Name)));
    }
}

public readonly record struct TenantId
{
    public string Value { get; }
    private TenantId(string value) => Value = value;
    public static bool TryCreate(string? raw, out TenantId tenant)
    {
        var value = raw?.Trim().ToLowerInvariant() ?? string.Empty;
        if (value.Length is >= 3 and <= 40 && value.All(character => char.IsAsciiLetterOrDigit(character) || character == '-'))
        { tenant = new TenantId(value); return true; }
        tenant = default; return false;
    }
    public override string ToString() => Value;
}

internal sealed class TenantContext
{
    public TenantId TenantId { get; private set; }
    public bool IsSet { get; private set; }
    public void Set(TenantId tenantId) { if (IsSet) throw new InvalidOperationException("Tenant context is immutable per request."); TenantId = tenantId; IsSet = true; }
}

internal sealed class TenantContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext http, TenantContext context)
    {
        var raw = http.User.FindFirstValue("tenant_id");
        if (!TenantId.TryCreate(raw, out var tenant)) { await http.ForbidAsync(); return; }
        context.Set(tenant);
        await next(http);
    }
}

public static class TenantCacheKey
{
    public static string For(string tenantId, string resource, Guid id)
    {
        if (!TenantId.TryCreate(tenantId, out var tenant)) throw new ArgumentException("Invalid tenant.", nameof(tenantId));
        return $"tenant:{tenant.Value}:{resource}:{id:N}:v1";
    }
}
