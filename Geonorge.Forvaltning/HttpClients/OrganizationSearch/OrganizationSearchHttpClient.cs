using Geonorge.Forvaltning.HttpClients.OrganizationSearch;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.HttpClients
{
    public class OrganizationSearchHttpClient(
        HttpClient client,
        OrganizationCache cache,
        IOptions<OrganizationSearchSettings> options) : IOrganizationSearchHttpClient
    {
        private readonly Uri _apiUrl = options.Value.ApiUrl;

        public async Task<string> SearchAsync(string orgNo)
        {
            var orgName = cache.GetOrganizationName(orgNo);

            if (orgName != null)
                return orgName;

            var organization = await GetResponseAsync(orgNo);

            if (organization == null)
                return null;

            orgName = organization["navn"].GetValue<string>();

            cache.AddOrganization(orgNo, orgName);

            return orgName;
        }

        private async Task<JsonNode> GetResponseAsync(string orgNo)
        {
            try
            {
                using var response = await client.GetAsync($"{_apiUrl}/{orgNo}");
                response.EnsureSuccessStatusCode();
                var body = await response.Content.ReadAsStringAsync();
                
                return JsonNode.Parse(body);
            }
            catch
            {
                return null;
            }
        }
    }
}
