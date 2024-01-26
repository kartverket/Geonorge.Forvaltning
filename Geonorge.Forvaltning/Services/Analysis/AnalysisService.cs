using GeoJSON.Text.Feature;
using GeoJSON.Text.Geometry;
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
    IAuthService authService,
    IOptions<AnalysisSettings> options,
    IOptions<DbConfiguration> dbOptions) : IAnalysisService
{
    const double EarthRadius = 6371008.8;

    public async Task<FeatureCollection> RunAnalysisAsync(int datasetId, AnalysisPayload payload)
    {
        var user = await authService.GetUserSupabaseAsync() ?? throw new Exception();

        if (!CanAnalyze(datasetId, payload, user))
            throw new Exception();

        var propertiesMetadata = await context.ForvaltningsObjektPropertiesMetadata
            .Where(metadata => metadata.ForvaltningsObjektMetadataId == payload.DatasetId)
            .AsNoTracking()
            .ToDictionaryAsync(grouping => grouping.ColumnName, grouping => grouping);

        await using var connection = new NpgsqlConnection(dbOptions.Value.ForvaltningApiDatabase);
        connection.Open();

        await using var command = new NpgsqlCommand();
        command.Connection = connection;
        command.CommandText = CreateSql(payload);

        await using var reader = await command.ExecuteReaderAsync();
        var features = new List<Feature>();

        while (await reader.ReadAsync())
        {
            using var document = await reader.GetFieldValueAsync<JsonDocument>(0);
            var feature = MapFeature(document, propertiesMetadata);

            features.Add(feature);
        }

        connection.Close();

        return new FeatureCollection(features);
    }

    private bool CanAnalyze(int datasetId, AnalysisPayload payload, User user)
    {
        return options.Value.Datasets
            .Any(dataset => dataset.Id == datasetId &&
                payload.DatasetId == dataset.AnalysisDatasetId &&
                dataset.Organizations.Contains(user.OrganizationNumber));
    }

    private static string CreateSql(AnalysisPayload payload)
    {
        var coords = payload.Point.Coordinates;
        var degrees = LengthToDegrees(payload.Distance);
        var wkt = $"POINT ({coords.Longitude} {coords.Latitude})";

        var whereClauses = payload.Filters
            .Select(filter =>
            {
                var jsonElement = (JsonElement)filter.Value;
                var value = jsonElement.ValueKind == JsonValueKind.String ? $"'{jsonElement}'" : jsonElement.ToString();

                return $" AND {filter.Property} = {value}";
            });

        return @$"
            SELECT row_to_json(row) FROM (
                SELECT public.t_{payload.DatasetId}.*, ST_Distance('{wkt}', geometry) AS distance
                FROM public.t_{payload.DatasetId}
                WHERE ST_DWithin(geometry, '{wkt}', {degrees}){string.Join("", whereClauses)}
                ORDER BY distance
                LIMIT {payload.Count}
            ) AS row;            
        ";
    }

    private static Feature MapFeature(JsonDocument document, Dictionary<string, ForvaltningsObjektPropertiesMetadata> propertiesMetadata)
    {
        var jsonObject = document.Deserialize<JsonObject>();

        var properties = new Dictionary<string, dynamic>()
        {
            { "id", jsonObject["id"].GetValue<int>()}
        };

        jsonObject.AsEnumerable()
            .Where(kvp => propertiesMetadata.ContainsKey(kvp.Key))
            .ToList()
            .ForEach(kvp =>
            {
                if (!propertiesMetadata.TryGetValue(kvp.Key, out var metadata))
                    return;

                dynamic property = new ExpandoObject();

                property.Name = metadata.Name;
                property.DataType = metadata.DataType;
                property.Value = kvp.Value;

                properties.Add(kvp.Key, property);
            });

        var geometry = JsonSerializer.Deserialize<IGeometryObject>(jsonObject["geometry"]);

        return new Feature(geometry, properties);
    }

    private static double LengthToDegrees(int distance)
    {
        var radians = distance / EarthRadius;
        var degrees = radians % (2 * Math.PI);

        return Math.Round(degrees * 180 / Math.PI, 6);
    }
}
