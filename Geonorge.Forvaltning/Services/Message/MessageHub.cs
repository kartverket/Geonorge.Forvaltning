using Geonorge.Forvaltning.Models.Api.Messaging;
using Geonorge.Forvaltning.Utils;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Geonorge.Forvaltning.Services.Message;

public class MessageHub : Hub<IMessageClient>
{
    private static readonly ConcurrentDictionary<string, string> _connectedUsers = [];
    private static readonly ConcurrentStack<string> _colors = GenerateColors();

    public async Task SendCursorMoved(string user, ConnectedUser message)
    {
        _connectedUsers.TryGetValue(user, out var color);
        message.Color = color;

        await Clients.AllExcept([user]).ReceiveCursorMoved(message);
    }

    public async Task SendObjectCreated(string user, CreatedObject message)
    {
        await Clients.AllExcept([user]).ReceiveObjectCreated(message);
    }

    public async Task SendObjectUpdated(string user, UpdatedObject message)
    {
        await Clients.AllExcept([user]).ReceiveObjectUpdated(message);
    }

    public async Task SendObjectsDeleted(string user, UpdatedObject message)
    {
        await Clients.AllExcept([user]).ReceiveObjectUpdated(message);
    }

    public override async Task OnConnectedAsync()
    {
        _colors.TryPop(out var color);
        _connectedUsers.TryAdd(Context.ConnectionId, color);

        await base.OnConnectedAsync();        
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {        
        await base.OnDisconnectedAsync(exception);

        var connectionId = Context.ConnectionId;

        _connectedUsers.TryRemove(connectionId, out var color);
        _colors.Push(color);

        await Clients.All.ReceiveUserDisconnected(connectionId);
    }

    private static ConcurrentStack<string> GenerateColors()
    {
        var colors = new ColorGenerator()
            .Generate("#86bff2", 20)
            .Darker(0.2)
            .Get();

        new Random().Shuffle(colors);

        return new ConcurrentStack<string>(colors);
    }
}
