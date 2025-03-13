using System.Net;
using Serilog.Context;
using Shared.Exceptions;

namespace Api.Middleware
{
    public class AuthenticationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthenticationMiddleware> _logger;

        public AuthenticationMiddleware(RequestDelegate next, ILogger<AuthenticationMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {

            await _next(context);
            if (context.Response.StatusCode == (int)HttpStatusCode.Forbidden)
            {
                await HandleExceptionAsync(context, new ForbiddenException("You do not have permission to access this resource."), (int)HttpStatusCode.Forbidden);
            }
            else if (context.Response.StatusCode == (int)HttpStatusCode.Unauthorized)
            {
                await HandleExceptionAsync(context, new UnauthorizedException("Unauthorized access. You must provide a valid token."), (int)HttpStatusCode.Unauthorized);
            }


        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception, int statusCode)
        {
            if (context.Response.HasStarted)
            {
                _logger.LogWarning("Response already started, skipping exception handling.");
                return;
            }

            var traceId = context.TraceIdentifier;
            using (LogContext.PushProperty("TraceId", traceId))
            {
                _logger.LogError(exception, "Authentication/Authorization error occurred. TraceId: {TraceId}", traceId);
            }

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = statusCode;

            var problemDetails = new
            {
                type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                title = exception.Message,
                status = statusCode,
                traceId
            };

            Console.WriteLine($"Returning error: {problemDetails.title} with status {statusCode}");

            await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(problemDetails));
        }

    }
}
