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

        [HttpPost("object", Name = "PostObject")]
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

        [HttpPost("object/{id:int}", Name = "PostObjectItem")]
        public async Task<IActionResult> PostObjectItem(int id, ObjectItem item)
        {
            //todo map dynamic object to database table
            throw new NotImplementedException();
        }

        [HttpGet("objects", Name = "GetMetadataObjects")]
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

        [HttpGet("object/{id:int}")]
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
    }
}
