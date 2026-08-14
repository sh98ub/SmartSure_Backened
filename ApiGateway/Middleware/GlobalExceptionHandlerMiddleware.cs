using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ApiGateway.Middleware
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
                _logger.LogError(ex, "API Gateway caught an unhandled exception: {Message}", ex.Message);
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var errorType = exception.GetType().Name;
            var message = "An internal API Gateway error occurred.";

            if (exception is HttpRequestException || exception is TaskCanceledException)
            {
                statusCode = HttpStatusCode.BadGateway;
                message = "Downstream microservice is unreachable or request timed out.";
            }
            else if (exception is KeyNotFoundException || exception is FileNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
                message = exception.Message;
            }
            else if (exception is ArgumentException || exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                statusCode = (int)statusCode,
                errorType = errorType,
                message = message,
                details = exception.Message,
                timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response);
            return context.Response.WriteAsync(json);
        }
    }
}
