using System.Collections.Concurrent;

namespace Geonorge.Forvaltning.HttpClients.OrganizationSearch
{
    public class OrganizationCache
    {
        private readonly ConcurrentDictionary<string, string> _cache = new();

        public string GetOrganizationName(string orgNo)
        {
            return _cache.TryGetValue(orgNo, out var orgName) ? orgName : null;
        }

        public void AddOrganization(string orgNo, string orgName)
        {
            _ = _cache.TryAdd(orgNo, orgName);
        }
    }
}
