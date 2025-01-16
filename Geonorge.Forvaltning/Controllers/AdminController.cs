using Geonorge.Forvaltning.Models.Api;
using Geonorge.Forvaltning.Services;
using Microsoft.AspNetCore.Mvc;

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
                var result = HandleException(ex);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpPut("object/{id:int}", Name = "PutObject")]
        public async Task<IActionResult> PutObject(int id, ObjectDefinitionEdit objekt)
        {
            try
            {
                return Ok(await _objectService.EditDefinition(id, objekt));
            }
            catch (Exception ex)
            {
                var result = HandleException(ex);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpDelete("object/{id:int}", Name = "DeleteObject")]
        public async Task<IActionResult> DeleteObject(int id)
        {
            try
            {
                await _objectService.DeleteObjectAsync(id);

                return NoContent();
            }
            catch (Exception ex)
            {
                var result = HandleException(ex);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpPost("access", Name = "PostAccess")]
        public async Task<IActionResult> PostAccess(ObjectAccess access)
        {
            try
            {
                return Ok(await _objectService.Access(access));
            }
            catch (Exception ex)
            {
                var result = HandleException(ex);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpPost("authorize-request", Name = "PostAuthorizeRequest")]
        public async Task<IActionResult> PostAuthorizeRequest()
        {
            try
            {
                await _objectService.RequestAuthorizationAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                var result = HandleException(ex);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpPut("tag/{datasetId:int}/{id:int}/{tag}", Name = "PutTag")]
        public async Task<IActionResult> PutTag(int datasetId, int id, string tag)
        {
            try
            {
                return Ok(await _objectService.EditTag(datasetId, id, tag));
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
