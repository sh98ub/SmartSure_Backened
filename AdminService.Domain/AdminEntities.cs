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

    public class SystemDashboardMetrics
    {
        public int TotalUsers { get; set; }
        public int ActivePolicies { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public decimal TotalPayouts { get; set; }
        public DateTime LastRefreshedAt { get; set; } = DateTime.UtcNow;
    }

    public class AdminUserOverview
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string KycStatus { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }
}
