using Geonorge.Forvaltning.Models.Api;
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

        [HttpGet(Name = "GetMetadataObjects")]
        public async Task<IActionResult> GetMetadataObjects()
        {
            try
            {
                return Ok(await _objectService.GetMetadataObjects());
            }
            catch (Exception ex)
            {
            }
            return BadRequest();
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetObject(int id)
        {
            try
            {
                return Ok(await _objectService.GetMetadataObject(id));
            }
            catch (Exception ex)
            {
            }
            return NotFound();
        }

        [HttpPost(Name = "PostObject")]
        public async Task<IActionResult> PostObject(ObjectDefinition objekt)
        {
            try
            {
                return Ok(await _objectService.AddDefinition(objekt));
            }
            catch (Exception ex)
            {
            }
            return BadRequest();
        }
    }
}
