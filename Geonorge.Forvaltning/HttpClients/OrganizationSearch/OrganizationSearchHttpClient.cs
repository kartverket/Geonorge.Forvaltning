using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json.Nodes;

namespace Geonorge.Forvaltning.HttpClients
{
    public class OrganizationSearchHttpClient(
        HttpClient httpClient,
        IMemoryCache memoryCache,
        IOptions<OrganizationSearchSettings> options) : IOrganizationSearchHttpClient
    {
        private readonly Uri _apiUrl = options.Value.ApiUrl;

        public async Task<string> SearchAsync(string orgNo)
        {
            var key = $"organization_{orgNo}";

            return await memoryCache.GetOrCreateAsync(key, async cacheEntry =>
            {
                cacheEntry.SlidingExpiration = TimeSpan.FromDays(30);

                var organization = await GetResponseAsync(orgNo);

                return organization["navn"]?.GetValue<string>();
            });
        }

        private async Task<JsonNode> GetResponseAsync(string orgNo)
        {
            try
            {
                using var response = await httpClient.GetAsync($"{_apiUrl}/{orgNo}");
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
