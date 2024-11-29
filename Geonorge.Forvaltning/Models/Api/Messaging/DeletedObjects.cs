namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class DeletedObjects : Message
{
    public List<int> Ids { get; set; }
}
