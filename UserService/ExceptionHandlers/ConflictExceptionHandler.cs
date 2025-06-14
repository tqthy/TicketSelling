using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using UserService.Exceptions;

namespace UserService.ExceptionHandlers
{ 
    public class ConflictExceptionHandler(ILogger<ConflictExceptionHandler> logger) : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is ConflictException conflictException)
            {
                var problemDetails = new ProblemDetails();
                problemDetails.Instance = httpContext.Request.Path;
                httpContext.Response.StatusCode = (int)conflictException.StatusCode;
                httpContext.Response.ContentType = "application/json";
                problemDetails.Title = conflictException.Message;
                problemDetails.Status = httpContext.Response.StatusCode;
                logger.LogError(exception, exception.Message);
                await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken).ConfigureAwait(false);
                return true;
            }
            return false;
        }
    }
}

