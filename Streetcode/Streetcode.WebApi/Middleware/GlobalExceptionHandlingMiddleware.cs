using System.Text.Json;
using Streetcode.BLL.Exceptions;

namespace Streetcode.WebApi.Middleware
{
    public class GlobalExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

        public GlobalExceptionHandlingMiddleware(
            RequestDelegate next,
            ILogger<GlobalExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            {
                try
                {
                    await _next(context);
                }
                catch (ValidationException ex)
                {
                    await HandleValidationAsync(context, ex);
                }
                catch (Exception ex)
                {
                    await HandleUnhandledAsync(context, ex);
                }
            }
        }

        private static Task HandleValidationAsync(HttpContext context, ValidationException exeption)
        {
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = StatusCodes.Status400BadRequest;

            var response = new
            {
                StatusCodes = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                errors = exeption.Errors.Select(e => new
                {
                    field = e.Key,
                    messages = e.Value
                })
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }

        private Task HandleUnhandledAsync(HttpContext context, Exception exception)
        {
            _logger.LogError(exception, "Unhandled exception");

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                statusCode = context.Response.StatusCode,
                message = "Internal server error"
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
