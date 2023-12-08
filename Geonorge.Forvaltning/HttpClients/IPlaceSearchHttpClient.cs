using GeoJSON.Text.Feature;

namespace Geonorge.Forvaltning.HttpClients
{
    public interface IPlaceSearchHttpClient
    {
        Task<FeatureCollection> SearchAsync(string searchString, int crs);
    }
}
