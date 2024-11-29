using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class CreatedObject : Message
{
    public JsonObject Object { get; set; }
}
