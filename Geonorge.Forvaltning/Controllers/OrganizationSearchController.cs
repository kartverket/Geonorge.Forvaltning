using Geonorge.Forvaltning.HttpClients;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Forvaltning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrganizationSearchController(
        IOrganizationSearchHttpClient organizationSearchHttpClient,
        ILogger<OrganizationSearchController> logger) : BaseController(logger)
    {
        [HttpGet("{orgNo}")]
        [ResponseCache(VaryByQueryKeys = new[] { "*" }, Duration = 86400)]
        public async Task<IActionResult> Search(string orgNo)
        {
            try
            {
                var result = await organizationSearchHttpClient.SearchAsync(orgNo);

                return Ok(result);
            }
            catch (Exception ex)
            {
                var result = HandleException(ex);

                if (result != null)
                    return result;

                throw;
            }
        }
    }
}
