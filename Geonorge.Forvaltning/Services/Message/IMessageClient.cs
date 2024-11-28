using Geonorge.Forvaltning.Models.Api;
using Geonorge.Forvaltning.Models.Api.Messaging;

namespace Geonorge.Forvaltning.Services.Message;

public interface IMessageClient
{
    Task ReceiveMessage(ConnectedUser message);
    Task ReceiveObjectUpdated(ObjectUpdated message);
}
