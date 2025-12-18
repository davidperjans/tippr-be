using API.Contracts.Errors;
using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult FromResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return MapFailure(result);
        }

        protected ActionResult FromResult<T>(Result<T> result)
        {
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return MapFailure(result);
        }

        private ActionResult MapFailure(Result result)
        {
            var error = result.Error ?? new Error(ErrorType.Failure, "Request failed.");

            var (status, rfcType) = MapStatus(error.Type);

            var response = new ErrorResponse
            {
                Type = rfcType,
                Title = error.Message,
                Status = status,
                Errors = error.ValidationErrors
            };

            return StatusCode(status, response);
        }

        private static (int Status, string RfcType) MapStatus(ErrorType type)
        {
            return type switch
            {
                ErrorType.Validation => (400, "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                ErrorType.BusinessRule => (400, "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                ErrorType.NotFound => (404, "https://tools.ietf.org/html/rfc7231#section-6.5.4"),
                ErrorType.Unauthorized => (401, "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
                ErrorType.Forbidden => (403, "https://tools.ietf.org/html/rfc7231#section-6.5.3"),
                ErrorType.Conflict => (409, "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
                _ => (400, "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
            };
        }
    }
}
