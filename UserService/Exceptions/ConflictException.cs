using System;
using System.Net;

namespace UserService.Exceptions
{
    public class ConflictException(string message, HttpStatusCode statusCode = HttpStatusCode.Conflict)
        : Exception(message)
    {
    public HttpStatusCode StatusCode { get; } = statusCode;
    }
}

