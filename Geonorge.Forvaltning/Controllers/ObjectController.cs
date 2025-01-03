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
        [HttpGet("objects/{datasetId:int}", Name = "Objects")]
        public async Task<IActionResult> Objects(int datasetId)
        {
            //try
            //{
                var result = await _objectService.GetObjects(datasetId);

                return Ok(result);
            //}
            //catch (Exception ex)
            //{
            //    var result = HandleException(ex);

            //    if (result != null)
            //        return result;

            //    throw;
            //}
        }
    }
}
