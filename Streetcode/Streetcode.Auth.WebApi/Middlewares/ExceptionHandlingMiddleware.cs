using System.Net;
using System.Text.Json;
using FluentValidation;
using Streetcode.Auth.BLL.Exceptions;

namespace Streetcode.Auth.WebApi.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            HttpStatusCode statusCode;
            object response;

            switch (exception)
            {
                case CustomException customEx:
                    statusCode = customEx.StatusCode;
                    response = new
                    {
                        StatusCode = (int)statusCode,
                        customEx.Message,
                        TraceId = context.TraceIdentifier
                    };
                    break;

                case ValidationException validationEx:
                    statusCode = HttpStatusCode.BadRequest;

                    var errors = validationEx.Errors
                        .GroupBy(e => e.PropertyName)
                        .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                    response = new
                    {
                        StatusCode = (int)statusCode,
                        Message = "Validation failed",
                        TraceId = context.TraceIdentifier,
                        Errors = errors
                    };
                    break;

                default:
                    statusCode = HttpStatusCode.InternalServerError;
                    _logger.LogError(exception, "Unhandled exception occurred.");

                    response = new
                    {
                        StatusCode = (int)statusCode,
                        Message = "An internal server error occurred.",
                        TraceId = context.TraceIdentifier
                    };
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}