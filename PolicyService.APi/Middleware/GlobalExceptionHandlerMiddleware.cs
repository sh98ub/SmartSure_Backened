using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using PolicyService.Domain.Exceptions;

namespace PolicyService.APi.Middleware
{
    public class GlobalExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

        public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var errorType = exception.GetType().Name;
            var message = exception.Message;

            switch (exception)
            {
                case PolicyPlanNotFoundException:
                case UserPolicyNotFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    break;
                case PolicyAlreadyCancelledException:
                case ArgumentException:
                case InvalidOperationException:
                    statusCode = HttpStatusCode.BadRequest;
                    break;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                statusCode = (int)statusCode,
                errorType = errorType,
                message = message,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
