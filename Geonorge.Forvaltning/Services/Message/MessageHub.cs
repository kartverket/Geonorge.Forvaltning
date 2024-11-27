using Geonorge.Forvaltning.Models.Api;
using Microsoft.AspNetCore.SignalR;

namespace Geonorge.Forvaltning.Services.Message;

public class MessageHub : Hub<IMessageClient>
{
    public async Task SendMessage(string user, ConnectedUser message)
    {
        await Clients.AllExcept([user]).ReceiveMessage(message);
    }
}
