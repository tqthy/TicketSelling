using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace BookingService.Api.Auth;

public class GatewayHeadersAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public GatewayHeadersAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // The ClaimsFromHeadersMiddleware has already run and set the HttpContext.User.
        // If it's authenticated, this handler can succeed.
        if (Context.User.Identity is { IsAuthenticated: true })
        {
            // The ticket is built from the existing principal, which was created in the middleware
            var ticket = new AuthenticationTicket(Context.User, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }

        // If the middleware didn't create an authenticated user, we have no result.
        return Task.FromResult(AuthenticateResult.NoResult());
    }
}