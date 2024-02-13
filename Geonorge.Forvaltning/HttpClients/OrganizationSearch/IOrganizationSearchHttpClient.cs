namespace Geonorge.Forvaltning.HttpClients;

public interface IOrganizationSearchHttpClient
{
    Task<string> SearchAsync(string orgNo);
}
