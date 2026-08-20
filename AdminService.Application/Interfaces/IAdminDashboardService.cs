using System.Collections.Generic;
using System.Threading.Tasks;
using AdminService.Application.DTOs;

namespace AdminService.Application.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<SystemDashboardMetricsDto> GetDashboardMetricsAsync();

        Task<IEnumerable<AdminUserOverviewDto>> GetUserOverviewListAsync();
        Task<AdminUserOverviewDto?> GetUserOverviewByIdAsync(int id);
        Task<AdminUserOverviewDto> UpdateUserStatusAsync(int id, AdminUpdateUserStatusDto dto);
        Task<object> GetUnapprovedClaimsAsync();
    }
}
