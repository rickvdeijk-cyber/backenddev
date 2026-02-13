using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace UserManagementAPI.Middleware;

public class TokenAuthenticationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenAuthenticationMiddleware> _logger;

    // For demo: hardcoded valid token
    // In real app → read from config, secrets, or validate JWT properly
    private const string ValidToken = "supersecret-token-12345";  // CHANGE THIS!

    public TokenAuthenticationMiddleware(RequestDelegate next, ILogger<TokenAuthenticationMiddleware> logger)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip auth check for public endpoints (e.g. login, health, swagger in dev)
        var path = context.Request.Path.Value?.ToLowerInvariant();
        var method = context.Request.Method.ToUpperInvariant();

        if (path?.StartsWith("/api/auth") == true ||    // future login endpoint
            path?.StartsWith("/swagger") == true ||
            path?.StartsWith("/health") == true ||
            (path?.StartsWith("/api/users") == true && method == "GET"))    // allow GET users only
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

        // Expect "Bearer <token>"
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

        // Token is valid → set user for [Authorize] and continue
        var claims = new[] { new Claim(ClaimTypes.Name, "api-user") };
        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);

        await _next(context);
    }
}