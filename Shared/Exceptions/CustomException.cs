namespace Shared.Exceptions
{
    public abstract class CustomException : Exception
    {
        public int StatusCode { get; }
        public string ErrorType { get; }

        protected CustomException(string message, int statusCode, string errorType) : base(message)
        {
            StatusCode = statusCode;
            ErrorType = errorType;
        }
    }

    public class NotFoundException : CustomException
    {
        public NotFoundException(string message = "The requested resource was not found.")
            : base(message, 404, "https://tools.ietf.org/html/rfc9110#section-15.5.5") { }
    }

    public class UnauthorizedException : CustomException
    {
        public UnauthorizedException(string message = "Unauthorized access.")
            : base(message, 401, "https://tools.ietf.org/html/rfc9110#section-15.5.2") { }
    }

    public class BadRequestException : CustomException
    {
        public BadRequestException(string message = "Bad request.")
            : base(message, 400, "https://tools.ietf.org/html/rfc9110#section-15.5.1") { }
    }
    public class ForbiddenException : CustomException
    {
        public ForbiddenException(string message = "Forbidden access.")
            : base(message, 403, "https://tools.ietf.org/html/rfc9110#section-15.5.4") { }
    }
}
