using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [ApiController]
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult<Result> FromResult(Result result)
        => result.IsSuccess ? Ok(result) : MapFailure(result);

        protected ActionResult<Result<T>> FromResult<T>(Result<T> result)
            => result.IsSuccess ? Ok(result) : MapFailure(result);

        private ActionResult MapFailure(Result result)
        {
            // Minimal heuristik tills du har ErrorCodes / ErrorType
            var message = result.Error ?? "Request failed.";

            if (LooksUnauthorized(message)) return Unauthorized(result);
            if (LooksNotFound(message)) return NotFound(result);

            return BadRequest(result);
        }

        private static bool LooksUnauthorized(string message)
        {
            message = message.ToLowerInvariant();
            return message.Contains("unauthorized")
                || message.Contains("not authenticated")
                || message.Contains("missing 'sub'")
                || message.Contains("missing 'email'")
                || message.Contains("invalid 'sub'");
        }

        private static bool LooksNotFound(string message)
        {
            message = message.ToLowerInvariant();
            return message.Contains("not found")
                || message.Contains("does not exist")
                || message.Contains("user not synced");
        }
    }
}
