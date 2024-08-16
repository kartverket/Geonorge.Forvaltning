using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
using Geonorge.Forvaltning.HttpClients;
using Geonorge.Forvaltning.Models.Api.Analysis;
using Geonorge.Forvaltning.Models.Api.User;
using Geonorge.Forvaltning.Models.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using System.Dynamic;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.Services;

public class AnalysisService(
    ApplicationContext context,
    NpgsqlConnection connection,
    IRouteSearchHttpClient routeSearchHttpClient,
    IAuthService authService,
    IOptions<AnalysisSettings> options,
    IOptions<DbConfiguration> dbOptions) : IAnalysisService
{

    public async Task<FeatureCollection> AnalyzeAsync(AnalysisPayload payload)
    {
        var user = await authService.GetUserFromSupabaseAsync() ??
            throw new UnauthorizedAccessException("Manglende eller feil autorisering");

        if (!CanAnalyze(payload, user))
            throw new AuthorizationException("Brukeren er ikke autorisert");

        var start = await GetStartAsync(payload);
        var startPoint = (Point)start.Geometry;

        var features = new List<Feature> { start };
        var destinations = await GetDestinationsAsync(payload, startPoint);

        var tasks = destinations
            .Select(feature =>
            {
                var id = (dynamic)feature.Properties["id"];
                var destinationId = (int)id.Value;
                var destination = (Point)feature.Geometry;

                return routeSearchHttpClient.SearchAsync(startPoint, destination, destinationId);
            });

        var routes = await Task.WhenAll(tasks);

        features.AddRange(destinations);
        features.AddRange(routes);

        return new FeatureCollection(features);
    }

    public async Task<List<int>> GetAnalysableDatasetIdsAsync(int datasetId)
    {
        var user = await authService.GetUserFromSupabaseAsync() ??
            throw new Exception();

        return options.Value.Datasets
            .Where(dataset => dataset.Id == datasetId &&
                dataset.Organizations.Contains(user.OrganizationNumber))
            .Select(dataset => dataset.AnalysisDatasetId)
            .ToList();
    }

    private async Task<Feature> GetStartAsync(AnalysisPayload payload)
    {
        var propertiesMetadata = await GetPropertiesMetadataAsync(payload.DatasetId);
        
        var sql = @$"
            SELECT row_to_json(row) FROM (
                SELECT * FROM public.t_{payload.DatasetId} WHERE id = {payload.ObjectId}
            ) AS row;
        ";

        connection.Open();

        await using var command = new NpgsqlCommand();
        command.Connection = connection;
        command.CommandText = sql;

        await using var reader = await command.ExecuteReaderAsync();
        Feature feature = null;

        if (reader.HasRows)
        {
            await reader.ReadAsync();
            using var document = await reader.GetFieldValueAsync<JsonDocument>(0);
            feature = MapFeature(document, propertiesMetadata, "start");
        }

        connection.Close();

        return feature;
    }

    private async Task<List<Feature>> GetDestinationsAsync(AnalysisPayload payload, Point startPoint)
    {
        var propertiesMetadata = await GetPropertiesMetadataAsync(payload.TargetDatasetId);

        connection.Open();

        await using var command = new NpgsqlCommand();
        command.Connection = connection;
        command.CommandText = CreateDestinationsSql(payload, startPoint);

        await using var reader = await command.ExecuteReaderAsync();
        var features = new List<Feature>();

        while (await reader.ReadAsync())
        {
            using var document = await reader.GetFieldValueAsync<JsonDocument>(0);
            var feature = MapFeature(document, propertiesMetadata, "destination");

            features.Add(feature);
        }

        connection.Close();

        return features;
    }

    private bool CanAnalyze(AnalysisPayload payload, User user)
    {
        return options.Value.Datasets
            .Any(dataset => dataset.Id == payload.DatasetId &&
                dataset.AnalysisDatasetId == payload.TargetDatasetId &&
                dataset.Organizations.Contains(user.OrganizationNumber));
    }

    private async Task<Dictionary<string, ForvaltningsObjektPropertiesMetadata>> GetPropertiesMetadataAsync(int datasetId)
    {
        return await context.ForvaltningsObjektPropertiesMetadata
            .Where(metadata => metadata.ForvaltningsObjektMetadataId == datasetId)
            .AsNoTracking()
            .ToDictionaryAsync(metadata => metadata.ColumnName, metadata => metadata);
    }

    private static string CreateDestinationsSql(AnalysisPayload payload, Point startPoint)
    {
        var coords = startPoint.Coordinates;
        var wkt = FormattableString.Invariant($"POINT ({coords.Longitude} {coords.Latitude})");
        var distance = payload.Distance * 1000;

        var whereClauses = payload.Filters
            .Select(filter =>
            {
                var value = "null";

                if (filter.Value.ValueKind != JsonValueKind.Null)
                    value = filter.Value.ValueKind == JsonValueKind.String ? $"'{filter.Value}'" : filter.Value.ToString();

                return $" AND {filter.Property} = {value}";
            });

        return @$"
            SELECT row_to_json(row) FROM (
                SELECT public.t_{payload.TargetDatasetId}.*, ST_Distance('{wkt}'::geography, geometry) AS distance
                FROM public.t_{payload.TargetDatasetId}
                WHERE ST_DWithin(geometry, '{wkt}'::geography, {distance}){string.Join("", whereClauses)}
                ORDER BY distance
                LIMIT {payload.Count}
            ) AS row;            
        ";
    }

    private static Feature MapFeature(
        JsonDocument document, Dictionary<string, ForvaltningsObjektPropertiesMetadata> propertiesMetadata, string type)
    {
        var jsonObject = document.Deserialize<JsonObject>();
        var featureId = jsonObject["id"].GetValue<int>();

        var properties = new Dictionary<string, dynamic>()
        {
            { "id", CreateProperty("ID", null, featureId) }
        };

        jsonObject.AsEnumerable()
            .Where(kvp => propertiesMetadata.ContainsKey(kvp.Key))
            .ToList()
            .ForEach(kvp =>
            {
                if (propertiesMetadata.TryGetValue(kvp.Key, out var metadata))
                    properties.Add(kvp.Key, CreateProperty(metadata.Name, metadata.DataType, kvp.Value));
            });

        properties.Add("_type", type);

        var geometry = JsonSerializer.Deserialize<IGeometryObject>(jsonObject["geometry"]);

        return new Feature(geometry, properties);
    }

    private static dynamic CreateProperty(string name, string dataType, dynamic value)
    {
        dynamic property = new ExpandoObject();

        property.Name = name;
        property.DataType = dataType;
        property.Value = value;

        return property;
    }
}
