using System.Net;
using System.Text.Json;
using Serilog.Context;
using Shared.Exceptions;

namespace Api.Middleware
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
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
            var traceId = context.TraceIdentifier;
            using (LogContext.PushProperty("TraceId", traceId))
            {
                _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);
            }

            var response = context.Response;
            response.ContentType = "application/json";

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                title = "An error occurred while processing your request.",
                status = (int)HttpStatusCode.InternalServerError,
                traceId
            };

            if (exception is CustomException customException)
            {
                response.StatusCode = customException.StatusCode;
                problemDetails = new
                {
                    type = customException.ErrorType,
                    title = customException.Message,
                    status = customException.StatusCode,
                    traceId
                };
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }

            var json = JsonSerializer.Serialize(problemDetails);
            await response.WriteAsync(json);
        }
    }
}
