namespace Geonorge.Forvaltning.Models.Api.Messaging;

public abstract class Message
{
    public string ConnectionId { get; set; }
    public string Username { get; set; }
    public int DatasetId { get; set; }
    public int? ObjectId { get; set; }
}
