using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SignalROperationsRoom;

public sealed record OperationMessage(string Room, string Sender, string Text, DateTimeOffset SentAtUtc);
public sealed record RoomSnapshot(string Room, IReadOnlyList<OperationMessage> Messages);

public interface IOperationsClient
{
    Task Snapshot(RoomSnapshot snapshot);
    Task MessageReceived(OperationMessage message);
}

[Authorize]
public sealed class OperationsHub(OperationsRoomStore store) : Hub<IOperationsClient>
{
    public async Task JoinRoom(string room)
    {
        room = OperationsRoomStore.ValidateRoom(room);
        await Groups.AddToGroupAsync(Context.ConnectionId, room);
        await Clients.Caller.Snapshot(new RoomSnapshot(room, store.Read(room)));
    }

    public async Task SendMessage(string room, string text)
    {
        room = OperationsRoomStore.ValidateRoom(room);
        text = text.Trim();
        if (text.Length is < 1 or > 500) throw new HubException("Message length must be between 1 and 500 characters.");
        var sender = Context.UserIdentifier ?? throw new HubException("The connection has no user identifier.");
        var message = store.Add(room, sender, text);
        await Clients.Group(room).MessageReceived(message);
    }
}

public sealed class OperationsRoomStore
{
    private readonly object _gate = new();
    private readonly Dictionary<string, List<OperationMessage>> _messages = new(StringComparer.OrdinalIgnoreCase);

    public OperationMessage Add(string room, string sender, string text)
    {
        lock (_gate)
        {
            if (!_messages.TryGetValue(room, out var list)) _messages[room] = list = [];
            var message = new OperationMessage(room, sender, text, DateTimeOffset.UtcNow);
            list.Add(message);
            if (list.Count > 50) list.RemoveAt(0);
            return message;
        }
    }

    public IReadOnlyList<OperationMessage> Read(string room)
    {
        lock (_gate) return _messages.TryGetValue(room, out var list) ? list.ToArray() : [];
    }

    public static string ValidateRoom(string room)
    {
        room = room.Trim().ToLowerInvariant();
        if (room.Length is < 3 or > 40 || room.Any(character => !char.IsAsciiLetterOrDigit(character) && character != '-'))
            throw new HubException("Room must be 3-40 ASCII letters, digits, or hyphens.");
        return room;
    }
}
