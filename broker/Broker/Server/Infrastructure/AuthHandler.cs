using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using System.Text;

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
        if (_configuration["brokerKey"] == null || _configuration["brokerKey"]!.Length == 0)
        {
            throw new Exception("Broker auth key not set");
        }

        var brokerKey = Convert.ToBase64String(Encoding.UTF8.GetBytes(_configuration["brokerKey"]!));
        var authorizeHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (authorizeHeader == null || authorizeHeader.Length < 6 || authorizeHeader.Substring(6) != brokerKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return;
        }

        await next(context);
    }
}
