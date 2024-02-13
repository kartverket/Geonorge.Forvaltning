using GeoJSON.Text.Feature;
using Geonorge.Forvaltning.Models.Api.Analysis;

namespace Geonorge.Forvaltning.Services;

public interface IAnalysisService
{
    Task<FeatureCollection> AnalyzeAsync(AnalysisPayload payload);
    Task<List<int>> GetAnalysableDatasetIdsAsync(int datasetId);
}
