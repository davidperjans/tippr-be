using API.Contracts.Errors;
using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    /// <summary>
    /// Base controller providing standardized Result-to-ActionResult mapping for all API endpoints.
    /// All controllers inherit from this to ensure consistent response formatting and error handling.
    /// </summary>
    /// <remarks>
    /// <para><b>Response Format</b></para>
    /// <para>All endpoints return responses wrapped in a Result object:</para>
    /// <code>
    /// // Success response
    /// {
    ///   "isSuccess": true,
    ///   "data": { ... },
    ///   "error": null
    /// }
    ///
    /// // Error response (RFC 7807 Problem Details format)
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    ///   "title": "Resource not found",
    ///   "status": 404,
    ///   "errors": null
    /// }
    /// </code>
    ///
    /// <para><b>Error Status Code Mapping</b></para>
    /// <list type="bullet">
    ///   <item><description>400 Bad Request: Validation errors, business rule violations</description></item>
    ///   <item><description>401 Unauthorized: Missing or invalid authentication</description></item>
    ///   <item><description>403 Forbidden: Insufficient permissions</description></item>
    ///   <item><description>404 Not Found: Resource does not exist</description></item>
    ///   <item><description>409 Conflict: Resource state conflict (e.g., duplicate entry)</description></item>
    /// </list>
    /// </remarks>
    [ApiController]
    [Produces("application/json")]
    public abstract class BaseApiController : ControllerBase
    {
        /// <summary>
        /// Maps a Result to an appropriate ActionResult with correct HTTP status code.
        /// </summary>
        protected ActionResult FromResult(Result result)
        {
            if (result.IsSuccess)
            {
                return Ok(result);
            }

            return MapFailure(result);
        }

        /// <summary>
        /// Maps a Result&lt;T&gt; to an appropriate ActionResult with correct HTTP status code.
        /// </summary>
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
