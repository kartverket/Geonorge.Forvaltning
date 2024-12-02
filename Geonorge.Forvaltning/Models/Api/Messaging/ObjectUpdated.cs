using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class ObjectUpdated : Message
{
    public int ObjectId { get; set; }
    public JsonObject Properties { get; set; }
}
