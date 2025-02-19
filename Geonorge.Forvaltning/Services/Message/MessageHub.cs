using Geonorge.Forvaltning.Models.Api.Messaging;
using Geonorge.Forvaltning.Utils;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace Geonorge.Forvaltning.Services.Message;

public class MessageHub : Hub<IMessageClient>
{
    private static readonly ConcurrentDictionary<string, string> _connectedUsers = [];
    private static readonly ConcurrentDictionary<string, ObjectEdited> _editedObjects = [];
    private static readonly ConcurrentStack<string> _colors = GenerateColors();

    public async Task SendPointerMoved(string user, PointerMoved message)
    {
        AddUserColor(user, message);

        await Clients.AllExcept([user]).ReceivePointerMoved(message);
    }

    public async Task SendObjectEdited(string user, ObjectEdited message)
    {
        AddUserColor(user, message);
        AddEditedObject(user, message);

        await SendObjectsEdited();
    }

    public async Task SendObjectCreated(string user, ObjectCreated message)
    {
        message.Object = null; // Todo get id from object
        await Clients.AllExcept([user]).ReceiveObjectCreated(message);
    }

    public async Task SendObjectUpdated(string user, ObjectUpdated message)
    {
        message.Properties = null; //do not send properties raw to clients, let clients fetch them from server, to avoid sending sensitive data, also could be done here if access to user tokens
        await Clients.AllExcept([user]).ReceiveObjectUpdated(message);
    }

    public async Task SendObjectsDeleted(string user, ObjectsDeleted message)
    {
        await Clients.AllExcept([user]).ReceiveObjectsDeleted(message);
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();

        AddConnectedUser();

        var objectsEdited = _editedObjects
            .Select(user => user.Value)
            .ToList();

        await Clients.Client(Context.ConnectionId).ReceiveObjectsEdited(objectsEdited);
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        await base.OnDisconnectedAsync(exception);

        RemoveEditedObject(Context.ConnectionId);
        RemoveConnectedUser();

        await Clients.All.ReceiveUserDisconnected(Context.ConnectionId);
        await SendObjectsEdited();
    }

    private async Task SendObjectsEdited()
    {
        var objectsEdited = _editedObjects
            .Select(user => user.Value)
            .ToList();

        await Clients.All.ReceiveObjectsEdited(objectsEdited);
    }

    private void AddConnectedUser()
    {
        _colors.TryPop(out var color);
        _connectedUsers.TryAdd(Context.ConnectionId, color);
    }

    private void RemoveConnectedUser()
    {
        _connectedUsers.TryRemove(Context.ConnectionId, out var color);
        _colors.Push(color);
    }

    private static void AddUserColor(string user, UserMessage message)
    {
        _connectedUsers.TryGetValue(user, out var color);
        message.Color = color;
    }

    private static void AddEditedObject(string user, ObjectEdited message)
    {
        if (!_editedObjects.ContainsKey(user) && message.ObjectId.HasValue)
            _editedObjects.TryAdd(user, message);
        else if (!message.ObjectId.HasValue)
            _editedObjects.TryRemove(user, out _);
    }

    private static void RemoveEditedObject(string user)
    {
        _editedObjects.TryRemove(user, out _);
    }

    private static ConcurrentStack<string> GenerateColors()
    {
        var colors = new ColorGenerator()
            .Generate("#86bff2", 20)
            .Darker(0.2)
            .Get();

        return new ConcurrentStack<string>(colors);
    }
}
