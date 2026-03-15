namespace Maba.Api.Middleware;

/// <summary>
/// Adds CORS headers at the start of the request so they are present on every response (including errors).
/// Runs before any other middleware that might write to the response.
/// </summary>
public class CorsHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public CorsHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var origin = context.Request.Headers.Origin.FirstOrDefault();
        var isAllowed = !string.IsNullOrEmpty(origin) && IsAllowedOrigin(origin);

        if (isAllowed)
        {
            context.Response.Headers.Append("Access-Control-Allow-Origin", origin);
            context.Response.Headers.Append("Access-Control-Allow-Credentials", "true");
        }
        context.Response.Headers.Append("Access-Control-Allow-Methods", "GET, POST, PUT, PATCH, DELETE, OPTIONS");
        context.Response.Headers.Append("Access-Control-Allow-Headers", "Content-Type, Authorization, Accept, X-Requested-With");
        context.Response.Headers.Append("Access-Control-Max-Age", "86400");

        if (context.Request.Method == "OPTIONS")
        {
            context.Response.StatusCode = 204;
            return;
        }

        await _next(context);
    }

    private static bool IsAllowedOrigin(string origin)
    {
        try
        {
            var uri = new Uri(origin);
            return uri.Scheme == "http" && (uri.Host == "localhost" || uri.Host == "127.0.0.1");
        }
        catch
        {
            return false;
        }
    }
}
