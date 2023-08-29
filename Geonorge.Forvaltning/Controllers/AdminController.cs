using Geonorge.Forvaltning.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Geonorge.Forvaltning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly IObjectService _objectService;
        private readonly IAuthService _authService;

        public AdminController(IObjectService objectService, IAuthService authService)
        {
            _objectService = objectService;
            _authService = authService;
        }

        [HttpPost(Name = "PostObject")]
        public async Task<IActionResult> Get(object o)
        {
            try
            {
                return Ok(await _objectService.AddDefinition(o));
            }
            catch (Exception ex)
            {
            }
            return BadRequest();
        }
    }
}
