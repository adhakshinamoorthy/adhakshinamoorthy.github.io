using Microsoft.AspNetCore.Authentication;
using SignalROperationsRoom;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddAuthentication("LocalHeader").AddScheme<AuthenticationSchemeOptions, HeaderAuthenticationHandler>("LocalHeader", _ => { });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<OperationsRoomStore>();
builder.Services.AddSignalR(options =>
{
    options.MaximumReceiveMessageSize = 16 * 1024;
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaximumParallelInvocationsPerClient = 1;
});

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapHub<OperationsHub>("/hubs/operations");
app.MapGet("/", () => Results.Ok(new { hub = "/hubs/operations", authentication = "X-User-Id (local sample only)" }));
app.Run();

public partial class Program;
