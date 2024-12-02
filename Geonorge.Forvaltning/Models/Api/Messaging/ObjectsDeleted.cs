namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class ObjectsDeleted : Message
{
    public List<int> Ids { get; set; }
}
