using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.HttpClients
{
    public class PlaceSearchHttpClient(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        IOptions<PlaceSearchSettings> options) : IPlaceSearchHttpClient
    {
        private readonly Uri _apiUrl = options.Value.ApiUrl;

        public async Task<FeatureCollection> SearchAsync(string searchString, int crs)
        {
            var key = $"place_{searchString.ToLower()}_{crs}";

            return await memoryCache.GetOrCreateAsync(key, async cacheEntry =>
            {
                cacheEntry.SlidingExpiration = TimeSpan.FromDays(1);

                var jsonNode = await GetResponseAsync(searchString, crs);

                return MapToFeatureCollection(jsonNode);
            });
        }

        private async Task<JsonNode> GetResponseAsync(string searchString, int crs)
        {
            try
            {
                using var response = await httpClient.GetAsync($"{_apiUrl}&sok={searchString}*&utkoordsys={crs}");
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                
                return JsonNode.Parse(body);
            }
            catch
            {
                return null;
            }
        }

        private static FeatureCollection MapToFeatureCollection(JsonNode jsonNode)
        {
            if (jsonNode == null)
                return new FeatureCollection();

            var features = jsonNode["navn"]
                .AsArray()
                .Select(node =>
                {
                    var id = node["stedsnummer"].GetValue<int>();
                    var lat = node["representasjonspunkt"]["nord"].GetValue<double>();
                    var lon = node["representasjonspunkt"]["øst"].GetValue<double>();

                    var properties = new Dictionary<string, object>
                    {
                        { "name", node["stedsnavn"]
                            .AsArray()
                            .FirstOrDefault(node => node["navnestatus"].GetValue<string>() == "hovednavn")?["skrivemåte"]
                            .GetValue<string>()
                        },
                        { "objectType", node["navneobjekttype"].GetValue<string>() },
                        { "municipality", node["kommuner"][0]["kommunenavn"].GetValue<string>() },
                        { "county", node["fylker"][0]["fylkesnavn"].GetValue<string>() }
                    };

                    return new Feature(new Point(new Position(lat, lon)), properties, id.ToString());
                })
                .ToList();

            return new FeatureCollection(features);
        }
    }
}
