using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class ObjectCreated : Message
{
    public JsonObject Object { get; set; }
}
