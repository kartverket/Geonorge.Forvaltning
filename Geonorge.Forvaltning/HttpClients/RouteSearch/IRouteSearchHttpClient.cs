using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;

namespace Geonorge.Forvaltning.HttpClients;

public interface IRouteSearchHttpClient
{
    Task<Feature> SearchAsync(Point start, Point destination, int destinationId);
}
