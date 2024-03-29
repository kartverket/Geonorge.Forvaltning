﻿using Geonorge.Forvaltning.HttpClients;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Forvaltning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PlaceSearchController(
        IPlaceSearchHttpClient placeSearchHttpClient,
        ILogger<PlaceSearchController> logger) : BaseController(logger)
    {
        [HttpGet("{searchString}/{crs}")]
        [ResponseCache(VaryByQueryKeys = new[] { "*" }, Duration = 86400)]
        public async Task<IActionResult> Search(string searchString, int crs)
        {
            try
            {
                var result = await placeSearchHttpClient.SearchAsync(searchString, crs);

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
