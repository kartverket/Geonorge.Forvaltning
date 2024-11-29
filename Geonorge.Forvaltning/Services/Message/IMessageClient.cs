using Geonorge.Forvaltning.Models.Api.Messaging;

namespace Geonorge.Forvaltning.Services.Message;

public interface IMessageClient
{
    Task ReceiveUserDisconnected(string connectionId);
    Task ReceiveCursorMoved(ConnectedUser message);
    Task ReceiveObjectCreated(CreatedObject message);
    Task ReceiveObjectUpdated(UpdatedObject message);
    Task ReceiveObjectsDeleted(DeletedObjects message);
}
