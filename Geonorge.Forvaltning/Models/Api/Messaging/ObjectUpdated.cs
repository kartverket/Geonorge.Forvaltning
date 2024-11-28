using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.Models.Api;

public class ObjectUpdated
{
    public string ConnectionId { get; set; }
    public int ObjectId { get; set; }
    public int DatasetId { get; set; }
    public JsonObject Properties { get; set; }
}
