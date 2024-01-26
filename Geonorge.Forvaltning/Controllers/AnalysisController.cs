﻿using Geonorge.Forvaltning.Models.Api.Analysis;
using Geonorge.Forvaltning.Services;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Forvaltning.Controllers;

[ApiController]
[Route("[controller]")]
public class AnalysisController(
    IAnalysisService analysisService,
    ILogger<AnalysisController> logger) : BaseController(logger)
{
    [HttpPost("{datasetId}")]
    public async Task<IActionResult> Analyze(int datasetId, [FromBody] AnalysisPayload payload)
    {
        try
        {
            var result = await analysisService.AnalyzeAsync(datasetId, payload);

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

    [HttpGet("{datasetId}")]
    public async Task<IActionResult> GetAnalysableDatasetIds(int datasetId)
    {
        try
        {
            var result = await analysisService.GetAnalysableDatasetIdsAsync(datasetId);

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
