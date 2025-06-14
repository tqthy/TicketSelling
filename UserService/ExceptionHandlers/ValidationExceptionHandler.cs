using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UserService.Exceptions;

namespace UserService.ExceptionHandlers;

public class ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        if (exception is ValidationException validationException)
        {
            var problemDetails = new ProblemDetails();
            problemDetails.Instance = httpContext.Request.Path;
            httpContext.Response.StatusCode = (int)validationException.StatusCode;
            httpContext.Response.ContentType = "application/json";
            problemDetails.Title = validationException.Message;
            problemDetails.Status = httpContext.Response.StatusCode;
            logger.LogError(exception, exception.Message);
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);
            return true;
        }
        return false;
    }
}