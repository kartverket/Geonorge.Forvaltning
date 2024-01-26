using GeoJSON.Text.Feature;
using Geonorge.Forvaltning.Models.Api.Analysis;

namespace Geonorge.Forvaltning.Services
{
    public interface IAnalysisService
    {
        Task<FeatureCollection> RunAnalysisAsync(int datasetId, AnalysisPayload payload);
    }
}
