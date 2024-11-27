using Geonorge.Forvaltning.Models.Api;

namespace Geonorge.Forvaltning.Services.Message;

public interface IMessageClient
{
    Task ReceiveMessage(ConnectedUser message);
}
