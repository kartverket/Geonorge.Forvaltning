using Geonorge.Forvaltning.Models.Api.Messaging;

namespace Geonorge.Forvaltning.Services.Message;

public interface IMessageClient
{
    Task ReceiveUserDisconnected(string connectionId);
    Task ReceivePointerMoved(PointerMoved message);
    Task ReceiveObjectEdited(ObjectEdited message);
    Task ReceiveObjectsEdited(List<ObjectEdited> message);
    Task ReceiveObjectCreated(ObjectCreated message);
    Task ReceiveObjectUpdated(ObjectUpdated message);
    Task ReceiveObjectsDeleted(ObjectsDeleted message);
}
