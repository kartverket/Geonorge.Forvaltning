namespace Geonorge.Forvaltning.HttpClients;

public class RouteSearchSettings
{
    public static readonly string SectionName = "RouteSearch";
    public Uri ApiUrl { get; set; }
    public string ApiKey { get; set; }
}
