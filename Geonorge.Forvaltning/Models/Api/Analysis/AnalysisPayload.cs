using GeoJSON.Text.Geometry;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.Models.Api.Analysis;

public class AnalysisPayload
{
    public int DatasetId { get; set; }
    public int TargetDatasetId { get; set; }
    public int Distance { get; set; }
    public int Count { get; set; }
    public List<AnalysisPayloadFilter> Filters { get; set; } = [];
    public Point Point { get; set; }

    public class AnalysisPayloadFilter
    {
        public string Property { get; set; }
        public JsonElement Value { get; set; }
    }
}
