using System;

namespace AdminService.Domain
{
    public class AuditLog
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Actor { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string IpAddress { get; set; } = "127.0.0.1";
    }
}
