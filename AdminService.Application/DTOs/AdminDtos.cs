using System;
using System.ComponentModel.DataAnnotations;

namespace AdminService.Application.DTOs
{


    public class SystemDashboardMetricsDto
    {
        public int TotalUsers { get; set; }
        public int ActivePolicies { get; set; }
        public int PendingClaims { get; set; }
        public int ApprovedClaims { get; set; }
        public int RejectedClaims { get; set; }
        public decimal TotalPayouts { get; set; }
        public DateTime LastRefreshedAt { get; set; }
    }

    public class AdminUserOverviewDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }

    public class AdminUpdateUserStatusDto
    {
        public bool IsActive { get; set; }
    }
}
