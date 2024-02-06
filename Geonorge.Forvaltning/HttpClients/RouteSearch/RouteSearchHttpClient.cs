using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.HttpClients;

public class RouteSearchHttpClient(
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IOptions<RouteSearchSettings> options) : IRouteSearchHttpClient
{
    private const int MaxRequestPerMinute = 40;
    private const string RateLimitKey = "route_search_rate_limit";

    public async Task<Feature> SearchAsync(Point start, Point destination, int destinationId)
    {
        var key = GetKey(start, destination, destinationId);

        if (memoryCache.TryGetValue(key, out Feature feature))
            return feature;

        if (RateLimitExceeeded())
            return CreateEmptyFeature(destinationId, HttpStatusCode.TooManyRequests);

        using var response = await httpClient.PostAsync(options.Value.ApiUrl, CreateHttpContent(start, destination));

        if (response.StatusCode == HttpStatusCode.OK)
        {
            var featureCollection = await response.Content.ReadFromJsonAsync<FeatureCollection>();
            feature = MapFeature(featureCollection, destinationId);

            CacheFeature(key, feature);

            return feature;
        }
        else
        {
            feature = CreateEmptyFeature(destinationId, response.StatusCode);

            if (response.StatusCode == HttpStatusCode.NotFound)
                CacheFeature(key, feature);

            return feature;
        }
    }

    private bool RateLimitExceeeded()
    {
        var count = 1;

        if (memoryCache.TryGetValue(RateLimitKey, out int cachedCount))
            count = cachedCount + 1;

        memoryCache.Set(RateLimitKey, count, DateTime.Now.AddSeconds(60));

        return count > MaxRequestPerMinute;
    }

    private void CacheFeature(string key, Feature feature)
    {
        memoryCache.Set(key, feature, new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromDays(1) });
    }

    private static Feature MapFeature(FeatureCollection searchResult, int destinationId)
    {
        var feature = searchResult.Features.FirstOrDefault();

        if (feature == null)
            return null;

        var json = JsonSerializer.Serialize(feature.Properties);
        var resultProps = JsonSerializer.Deserialize<JsonNode>(json);

        var properties = new Dictionary<string, dynamic>
        {
            { "destinationId", destinationId },
            { "distance", resultProps["summary"]["distance"].GetValue<double>() },
            { "duration", resultProps["summary"]["duration"].GetValue<double>() },
            { "_type", "route" }
        };

        return new Feature(feature.Geometry, properties);
    }

    private static Feature CreateEmptyFeature(int destinationId, HttpStatusCode statusCode)
    {
        var properties = new Dictionary<string, dynamic>
        {
            { "id", destinationId },
            { "statusCode", (int)statusCode },
            { "_type", "route" }
        };

        return new Feature(null, properties);
    }

    private static StringContent CreateHttpContent(Point start, Point destination)
    {
        double[] startCoords = [start.Coordinates.Longitude, start.Coordinates.Latitude];
        double[] stopCoords = [destination.Coordinates.Longitude, destination.Coordinates.Latitude];

        var body = new Dictionary<string, double[][]>
        {
            { "coordinates", new double[][] { startCoords, stopCoords } }
        };

        return new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
    }

    private static string GetKey(Point start, Point destination, int destinationId)
    {
        var startCoords = start.Coordinates;
        var destCoords = destination.Coordinates;

        return $"route_{startCoords.Latitude}_{startCoords.Longitude}_{destCoords.Latitude}_{destCoords.Longitude}_{destinationId}";
    }
}
