using System.Net;

namespace UserService.Exceptions;

public class NotFoundException(string message, HttpStatusCode statusCode = HttpStatusCode.NotFound)
    : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
}