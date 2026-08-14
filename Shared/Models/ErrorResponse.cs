using System;

namespace Shared.Models
{
    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string ErrorType { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string CorrelationId { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
