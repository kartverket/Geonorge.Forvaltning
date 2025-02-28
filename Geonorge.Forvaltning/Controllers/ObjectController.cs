﻿using Geonorge.Forvaltning.Services;
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
        [HttpGet("{datasetId:int}/{objectId:int?}", Name = "Objects")]
        public async Task<IActionResult> Objects(int datasetId, int? objectId = null)
        {
            try
            {
                var result = await _objectService.GetObjects(datasetId, objectId);

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

        [HttpPost("{datasetId:int}", Name = "PostObjectData")]
        public async Task<IActionResult> PostObjectData(int datasetId, [FromBody] object objekt)
        {
            try
            {
                var result = await _objectService.PostObjectData(datasetId, objekt);

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

        [HttpPost("all/{datasetId:int}", Name = "PostAllObjectData")]
        public async Task<IActionResult> PostAllObjectData(int datasetId, [FromBody] List<object> objekts)
        {
            int rowsAffected = 0;
            try
            {
                rowsAffected = await _objectService.PostObjectsData(datasetId, objekts);

                return Ok(rowsAffected);
            }
            catch (Exception ex)
            {
                var result = HandleException(ex);

                if (result != null)
                    return result;

                throw;
            }
        }

        [HttpPut("{datasetId:int}", Name = "PutObjectData")]
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

        [HttpDelete("{datasetId:int}/{objektId:int}", Name = "DeleteObjectData")]
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

        [HttpDelete("{datasetId:int}", Name = "DeleteAllObjectData")]
        public async Task<IActionResult> DeleteAllObjectData(int datasetId)
        {
            try
            {
                var result = await _objectService.DeleteAllObjectData(datasetId);

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
