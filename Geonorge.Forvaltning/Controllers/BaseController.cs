using Geonorge.Forvaltning.Services;
using Microsoft.AspNetCore.Mvc;

namespace Geonorge.Forvaltning.Controllers
{
    public abstract class BaseController : ControllerBase
    {
        private readonly ILogger<ControllerBase> _logger;

        protected BaseController(
            ILogger<ControllerBase> logger)
        {
            _logger = logger;
        }

        protected IActionResult HandleException(Exception exception)
        {
            _logger.LogError(exception.ToString());

            switch (exception)
            {
                case ArgumentException _:
                case FormatException _:
                    return BadRequest();
                case UnauthorizedAccessException ex:
                    return StatusCode(StatusCodes.Status401Unauthorized, ex.Message);
                case AuthorizationException ex:
                    return StatusCode(StatusCodes.Status403Forbidden, ex.Message);
                case Exception _:
                    return StatusCode(StatusCodes.Status500InternalServerError);
            }

            return null;
        }
    }
}
