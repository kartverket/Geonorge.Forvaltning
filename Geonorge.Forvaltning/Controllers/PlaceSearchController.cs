using Geonorge.Forvaltning.HttpClients;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Forvaltning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaceSearchController : BaseController
    {
        private readonly IPlaceSearchHttpClient _placeSearchHttpClient;

        public PlaceSearchController(
            IPlaceSearchHttpClient placeSearchHttpClient, 
            ILogger<AdminController> logger) : base(logger)
        {
            _placeSearchHttpClient = placeSearchHttpClient;
        }

        [HttpGet("{searchString}/{crs}")]
        [ResponseCache(VaryByQueryKeys = new[] { "*" }, Duration = 86400)]
        public async Task<IActionResult> Search(string searchString, int crs)
        {
            try
            {
                var result = await _placeSearchHttpClient.SearchAsync(searchString, crs);

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
