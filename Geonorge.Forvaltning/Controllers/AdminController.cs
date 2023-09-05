using Geonorge.Forvaltning.Models.Api;
using Geonorge.Forvaltning.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Data;
using System.Dynamic;
using System.Text.Json;

namespace Geonorge.Forvaltning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AdminController : BaseController
    {
        private readonly IObjectService _objectService;
        private readonly IAuthService _authService;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IObjectService objectService, IAuthService authService, ILogger<AdminController> logger) : base(logger)
        {
            _objectService = objectService;
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("object", Name = "PostObject")]
        public async Task<IActionResult> PostObject(ObjectDefinitionAdd objekt)
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
            try
            {
                return Ok(await _objectService.AddObject(id, item));
            }
            catch (Exception ex)
            {
            }

            return BadRequest();
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
