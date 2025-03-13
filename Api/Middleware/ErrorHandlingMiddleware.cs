using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
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
            catch (DbUpdateException ex)
            {
                await HandleExceptionAsync(context, new NotFoundException("Database error occurred. Resource not found."), (int)HttpStatusCode.NotFound);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, int? statusCodeOverride = null)
        {
            var traceId = context.TraceIdentifier;
            using (LogContext.PushProperty("TraceId", traceId))
            {
                _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);
            }

            var response = context.Response;
            response.ContentType = "application/json";

            var statusCode = statusCodeOverride ?? (exception is CustomException customException ? customException.StatusCode : (int)HttpStatusCode.InternalServerError);

            response.StatusCode = statusCode;

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                title = exception.Message,
                status = statusCode,
                traceId
            };

            var json = JsonSerializer.Serialize(problemDetails);
            await response.WriteAsync(json);
        }
    }
}
