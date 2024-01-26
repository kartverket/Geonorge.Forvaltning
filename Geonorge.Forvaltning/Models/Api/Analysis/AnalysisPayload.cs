using GeoJSON.Text.Geometry;

namespace Geonorge.Forvaltning.Models.Api.Analysis;

public class AnalysisPayload
{
    public int DatasetId { get; set; }
    public int Distance { get; set; }
    public int Count { get; set; }
    public List<AnalysisPayloadFilter> Filters { get; set; } = [];
    public Point Point { get; set; }

    public class AnalysisPayloadFilter
    {
        public string Property { get; set; }
        public dynamic Value { get; set; }
    }
}
