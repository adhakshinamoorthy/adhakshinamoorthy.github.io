using Microsoft.AspNetCore.Http.Connections;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR.Client;
using Xunit;

namespace SignalROperationsRoom.Tests;

public sealed class OperationsHubTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    public OperationsHubTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task Anonymous_connection_is_rejected()
    {
        await using var connection = CreateConnection(null);
        await Assert.ThrowsAnyAsync<Exception>(() => connection.StartAsync());
    }

    [Fact]
    public async Task Join_returns_snapshot_and_group_receives_message()
    {
        await using var connection = CreateConnection("operator-1");
        var snapshot = new TaskCompletionSource<RoomSnapshot>(TaskCreationOptions.RunContinuationsAsynchronously);
        var received = new TaskCompletionSource<OperationMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
        connection.On<RoomSnapshot>("Snapshot", value => snapshot.TrySetResult(value));
        connection.On<OperationMessage>("MessageReceived", value => received.TrySetResult(value));
        await connection.StartAsync();
        await connection.InvokeAsync("JoinRoom", "incident-42");
        Assert.Equal("incident-42", (await snapshot.Task.WaitAsync(TimeSpan.FromSeconds(5))).Room);
        await connection.InvokeAsync("SendMessage", "incident-42", "Database recovered");
        var message = await received.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.Equal("operator-1", message.Sender);
        Assert.Equal("Database recovered", message.Text);
    }

    [Fact]
    public void Invalid_room_is_rejected_by_hub_contract()
    {
        Assert.Throws<HubException>(() => OperationsRoomStore.ValidateRoom("!!"));
    }

    private HubConnection CreateConnection(string? userId) => new HubConnectionBuilder()
        .WithUrl(new Uri(_factory.Server.BaseAddress, "/hubs/operations"), options =>
        {
            options.Transports = HttpTransportType.WebSockets;
            options.HttpMessageHandlerFactory = _ => _factory.Server.CreateHandler();
            options.WebSocketFactory = async (context, cancellationToken) =>
            {
                var webSocketClient = _factory.Server.CreateWebSocketClient();
                if (userId is not null) webSocketClient.ConfigureRequest = request => request.Headers["X-User-Id"] = userId;
                return await webSocketClient.ConnectAsync(context.Uri, cancellationToken);
            };
            if (userId is not null) options.Headers.Add("X-User-Id", userId);
        })
        .WithAutomaticReconnect([TimeSpan.Zero, TimeSpan.FromMilliseconds(100)])
        .Build();
}
