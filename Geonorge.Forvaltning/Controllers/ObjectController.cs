using Geonorge.Forvaltning.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Geonorge.Forvaltning.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ObjectController : BaseController
    {
        private readonly IObjectService _objectService;
        private readonly ILogger<AdminController> _logger;
        public ObjectController(IObjectService objectService, ILogger<AdminController> logger) : base(logger)
        {
            _objectService = objectService;
            _logger = logger;
        }
        [HttpGet("objects/{datasetId:int}", Name = "Objects")]
        public async Task<IActionResult> Objects(int datasetId)
        {
            try
            {
                var result = await _objectService.GetObjects(datasetId);

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

        [HttpPut("objects/{datasetId:int}", Name = "PutObjectData")]
        public async Task<IActionResult> PutObjectData(int datasetId, [FromBody]object objekt)
        {
            try
            {
                var result = await _objectService.PutObjectData(datasetId, objekt);

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

        [HttpDelete("objects/{datasetId:int}/{objektId:int}", Name = "DeleteObjectData")]
        public async Task<IActionResult> DeleteObjectData(int datasetId, int objektId)
        {
            try
            {
                var result = await _objectService.DeleteObjectData(datasetId, objektId);

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
