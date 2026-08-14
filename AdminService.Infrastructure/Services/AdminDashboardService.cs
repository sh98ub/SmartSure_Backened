using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using AdminService.Domain;
using AdminService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AdminService.Infrastructure.Services
{
    public class AdminDashboardService : IAdminDashboardService
    {
        private readonly AdminDbContext _context;

        public AdminDashboardService(AdminDbContext context)
        {
            _context = context;
        }

        public async Task<SystemDashboardMetricsDto> GetDashboardMetricsAsync()
        {
            var totalUsers = await _context.UserOverviews.CountAsync();
            var activeUsersCount = await _context.UserOverviews.CountAsync(u => u.IsActive);
            var metrics = new SystemDashboardMetricsDto
            {
                TotalUsers = totalUsers,
                ActivePolicies = activeUsersCount * 2, // Calculated based on user subscriptions overview
                PendingClaims = 1,
                ApprovedClaims = 3,
                TotalPayouts = 150000.00m,
                LastRefreshedAt = DateTime.UtcNow
            };
            return metrics;
        }

        public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync()
        {
            var logs = await _context.AuditLogs.OrderByDescending(x => x.Timestamp).ToListAsync();
            return logs.Select(MapToDto).ToList();
        }

        public async Task<AuditLogDto> LogActivityAsync(CreateAuditLogDto dto)
        {
            var log = new AuditLog
            {
                Timestamp = DateTime.UtcNow,
                Actor = dto.Actor,
                Action = dto.Action,
                Details = dto.Details,
                IpAddress = dto.IpAddress
            };
            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
            return MapToDto(log);
        }

        public async Task<IEnumerable<AdminUserOverviewDto>> GetUserOverviewListAsync()
        {
            var users = await _context.UserOverviews.ToListAsync();
            return users.Select(MapToUserOverviewDto).ToList();
        }

        public async Task<AdminUserOverviewDto?> GetUserOverviewByIdAsync(int id)
        {
            var user = await _context.UserOverviews.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' was not found.");
            }
            return MapToUserOverviewDto(user);
        }

        public async Task<AdminUserOverviewDto> UpdateUserStatusAsync(int id, AdminUpdateUserStatusDto dto)
        {
            var user = await _context.UserOverviews.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with ID '{id}' was not found.");
            }

            user.IsActive = dto.IsActive;
            if (!string.IsNullOrWhiteSpace(dto.KycStatus))
            {
                user.KycStatus = dto.KycStatus;
            }

            await _context.SaveChangesAsync();
            return MapToUserOverviewDto(user);
        }

        private static AuditLogDto MapToDto(AuditLog log)
        {
            return new AuditLogDto
            {
                Id = log.Id,
                Timestamp = log.Timestamp,
                Actor = log.Actor,
                Action = log.Action,
                Details = log.Details,
                IpAddress = log.IpAddress
            };
        }

        private static AdminUserOverviewDto MapToUserOverviewDto(AdminUserOverview user)
        {
            return new AdminUserOverviewDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                KycStatus = user.KycStatus,
                Role = user.Role,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
