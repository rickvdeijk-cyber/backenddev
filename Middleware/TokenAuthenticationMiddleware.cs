using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UserManagementAPI.Middleware;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;
    private const string ValidToken = "supersecret-token-12345";  

    public TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant();
        var method = context.Request.Method.ToUpperInvariant();

        if (path?.StartsWith("/api/auth") == true || 
            path?.StartsWith("/swagger") == true ||
            path?.StartsWith("/health") == true ||
            (path?.StartsWith("/api/users") == true && method == "GET"))  
        {
            await _next(context);
            return;
        }

        // Get token from Authorization header
        if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            _logger.LogWarning("Missing Authorization header");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing or invalid Authorization header.");
            return;
        }

        var headerValue = authHeader.ToString();
        _logger.LogInformation("Received Authorization header: '{Header}'", headerValue);

        if (!headerValue.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Invalid Authorization scheme (expected Bearer)");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Authorization scheme must be Bearer.");
            return;
        }

        var token = headerValue["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(token) || token != ValidToken)
        {
            _logger.LogWarning("Invalid or expired token provided");
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or expired token.");
            return;
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "api-user") };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        await _next(context);
    }
}