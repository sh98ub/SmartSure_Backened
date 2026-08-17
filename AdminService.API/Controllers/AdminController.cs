using System.Threading.Tasks;
using AdminService.Application.DTOs;
using AdminService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace AdminService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly IAdminDashboardService _adminService;

        public AdminController(IAdminDashboardService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("dashboard")]
        [EndpointSummary("[ADMIN ONLY] Get system dashboard metrics")]
        [EndpointDescription("Requires Admin role. Returns high-level dashboard statistics for the SmartSure platform.")]
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var metrics = await _adminService.GetDashboardMetricsAsync();
            return Ok(ApiResponse<SystemDashboardMetricsDto>.SuccessResponse(metrics, "Dashboard metrics retrieved successfully."));
        }

        [HttpGet("users")]
        [EndpointSummary("[ADMIN ONLY] Get all users overview")]
        [EndpointDescription("Requires Admin role. Returns a summary list of all registered users.")]
        public async Task<IActionResult> GetAllUsersOverview()
        {
            var users = await _adminService.GetUserOverviewListAsync();
            return Ok(ApiResponse<object>.SuccessResponse(users, "User overviews retrieved successfully."));
        }

        [HttpGet("users/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Get user overview by ID")]
        [EndpointDescription("Requires Admin role. Returns overview details for a specific user by ID.")]
        public async Task<IActionResult> GetUserOverviewById(int id)
        {
            var user = await _adminService.GetUserOverviewByIdAsync(id);
            return Ok(ApiResponse<AdminUserOverviewDto?>.SuccessResponse(user, "User overview retrieved successfully."));
        }

        [HttpPut("users/{id:int}/status")]
        [EndpointSummary("[ADMIN ONLY] Update user status")]
        [EndpointDescription("Requires Admin role. Updates the active/KYC status for a user.")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] AdminUpdateUserStatusDto dto)
        {
            var updatedUser = await _adminService.UpdateUserStatusAsync(id, dto);
            return Ok(ApiResponse<AdminUserOverviewDto>.SuccessResponse(updatedUser, "User status updated successfully."));
        }

        [HttpGet("claims/unapproved")]
        [EndpointSummary("[ADMIN ONLY] Get all unapproved claims")]
        [EndpointDescription("Requires Admin role. Returns all claims that are currently Submitted or UnderReview.")]
        public async Task<IActionResult> GetUnapprovedClaims()
        {
            var claims = await _adminService.GetUnapprovedClaimsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(claims, "Unapproved claims retrieved successfully."));
        }
    }
}
