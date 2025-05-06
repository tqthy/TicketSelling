// File: Middleware/ClaimsFromHeadersMiddleware.cs
using System.Security.Claims;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.Http; // Required for HttpContext extension methods if used
using Microsoft.Extensions.Logging; // Add for logging

namespace VenueService.Middleware
{
    public class ClaimsFromHeadersMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ClaimsFromHeadersMiddleware> _logger; // Inject Logger
        private const string UserIdHeader = "X-User-Id";
        private const string UserRolesHeader = "X-User-Roles";

        // Inject ILogger
        public ClaimsFromHeadersMiddleware(RequestDelegate next, ILogger<ClaimsFromHeadersMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation("ClaimsFromHeadersMiddleware executing for path: {Path}", context.Request.Path); // Log entry

            StringValues userIdValues = context.Request.Headers[UserIdHeader];
            StringValues userRolesValues = context.Request.Headers[UserRolesHeader];

            if (!StringValues.IsNullOrEmpty(userIdValues) && !StringValues.IsNullOrEmpty(userRolesValues))
            {
                string userId = userIdValues.First();
                string rolesHeaderValue = userRolesValues.First();
                _logger.LogInformation("Found headers: {UserIdHeader}={UserId}, {UserRolesHeader}={UserRoles}",
                    UserIdHeader, userId, UserRolesHeader, rolesHeaderValue);

                var claims = new List<Claim> { new Claim(ClaimTypes.NameIdentifier, userId) };
                if (!string.IsNullOrWhiteSpace(rolesHeaderValue))
                {
                    string[] roles = rolesHeaderValue.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    foreach (var role in roles) { claims.Add(new Claim(ClaimTypes.Role, role)); }
                }

                var identity = new ClaimsIdentity(claims, "GatewayHeaders"); // Use the explicit auth type name
                var principal = new ClaimsPrincipal(identity);
                context.User = principal;

                _logger.LogInformation("Set HttpContext.User. Identity Authenticated: {IsAuth}, Roles: {Roles}",
                    context.User.Identity?.IsAuthenticated, // Should be true now
                    string.Join(',', context.User.FindAll(ClaimTypes.Role).Select(c => c.Value)));
            }
            else
            {
                _logger.LogWarning("Required headers ({UserIdHeader}, {UserRolesHeader}) not found or empty.",
                    UserIdHeader, UserRolesHeader);
                 // context.User remains anonymous
                 _logger.LogInformation("HttpContext.User is anonymous. Identity Authenticated: {IsAuth}", context.User.Identity?.IsAuthenticated); // Should be false
            }

            await _next(context);
        }
    }
}