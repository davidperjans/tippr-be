using System.Text.Json;
using API.Contracts.Errors;
using Serilog;

namespace API.Middleware;

public sealed class ErrorHandlingMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unhandled exception. TraceId={TraceId} Path={Path}",
                context.TraceIdentifier, context.Request.Path);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 500;

            var response = new ErrorResponse
            {
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "An unexpected error occurred.",
                Status = 500,
                Errors = null
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(json);
        }
    }
}
