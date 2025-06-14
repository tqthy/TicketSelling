using System.Net;

namespace UserService.Exceptions;

public class ValidationException(string message, HttpStatusCode statusCode = HttpStatusCode.BadRequest)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}