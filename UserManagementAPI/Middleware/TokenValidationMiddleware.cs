using System.Text.Json;

public class TokenValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<TokenValidationMiddleware> _logger;

    public TokenValidationMiddleware(RequestDelegate next, ILogger<TokenValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Check for Authorization header
        if (!context.Request.Headers.TryGetValue("Authorization", out var token))
        {
            await WriteUnauthorizedResponse(context, "Missing Authorization header.");
            return;
        }

        // Simple validation logic (replace with real token validation, e.g., JWT)
        if (string.IsNullOrWhiteSpace(token) || !IsValidToken(token))
        {
            await WriteUnauthorizedResponse(context, "Invalid token.");
            return;
        }

        // Continue down the pipeline if valid
        await _next(context);
    }

    private bool IsValidToken(string token)
    {
        // Example: check for a static token value
        // In production, validate JWT or another secure token format
        return token == "Bearer my-secret-token";
    }

    private async Task WriteUnauthorizedResponse(HttpContext context, string message)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";

        var errorResponse = new { error = message };
        var json = JsonSerializer.Serialize(errorResponse);

        await context.Response.WriteAsync(json);
    }
}