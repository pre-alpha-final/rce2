using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace Broker.Server.Infrastructure;

public class AuthHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly IConfiguration _configuration;

    public AuthHandler(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        var authorizeHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authorizeHeader == null || authorizeHeader.Length < 6 || authorizeHeader.Substring(5) != _configuration["code"])
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}
