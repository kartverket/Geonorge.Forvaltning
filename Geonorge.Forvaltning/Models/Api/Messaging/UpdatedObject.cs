using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.Models.Api.Messaging;

public class UpdatedObject : Message
{
    public JsonObject Properties { get; set; }
}
