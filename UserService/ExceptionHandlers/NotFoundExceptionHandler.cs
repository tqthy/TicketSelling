using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UserService.Exceptions;

namespace UserService.ExceptionHandlers;

public class NotFoundExceptionHandler(ILogger<NotFoundExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is NotFoundException notFoundException)
        {
            var problemDetails = new ProblemDetails();
            problemDetails.Instance = httpContext.Request.Path;
            httpContext.Response.StatusCode = (int)notFoundException.StatusCode;
            httpContext.Response.ContentType = "application/json";
            problemDetails.Title = notFoundException.Message;
            problemDetails.Status = httpContext.Response.StatusCode;
            logger.LogError(exception, exception.Message);
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);
            return true;
        }
        return false;
    }
}