using Microsoft.AspNetCore.Builder;

namespace Shared.Middleware
{
    public static class MiddlewareExtensions
    {
        public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        {
            return app.UseMiddleware<CorrelationIdMiddleware>();
        }

        public static IApplicationBuilder UseSharedGlobalExceptionHandler(this IApplicationBuilder app)
        {
            return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
        }
    }
}
