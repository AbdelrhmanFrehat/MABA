using Maba.Api.Middleware;

namespace Maba.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseCorsHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CorsHeadersMiddleware>();
    }

    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}

