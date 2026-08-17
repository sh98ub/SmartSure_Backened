using System;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Shared.Models;

namespace Shared.Middleware
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
                var correlationId = context.Items.TryGetValue(CorrelationIdMiddleware.CorrelationIdHeader, out var val)
                    ? val?.ToString() ?? string.Empty
                    : string.Empty;

                _logger.LogError(ex, "[CorrelationId: {CorrelationId}] An unhandled exception occurred: {Message}", correlationId, ex.Message);
                await HandleExceptionAsync(context, ex, correlationId);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception, string correlationId)
        {
            context.Response.ContentType = "application/json";

            var statusCode = HttpStatusCode.InternalServerError;
            var errorType = exception.GetType().Name;
            var message = exception.Message;

            if (exception.GetType().Name.Contains("NotFoundException") || exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (exception.GetType().Name.Contains("InvalidCredentialsException") || exception is UnauthorizedAccessException)
            {
                statusCode = HttpStatusCode.Unauthorized;
            }
            else if (exception.GetType().Name.Contains("AlreadyExistsException") || 
                     exception.GetType().Name.Contains("InvalidClaimAmountException") ||
                     exception.GetType().Name.Contains("PolicyAlreadyCancelledException") ||
                     exception.GetType().Name.Contains("Invalid") || 
                     exception.GetType().Name.Contains("Already") || 
                     exception is ArgumentException || 
                     exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new ErrorResponse
            {
                StatusCode = (int)statusCode,
                ErrorType = errorType,
                Message = message,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            return context.Response.WriteAsync(json);
        }
    }
}
