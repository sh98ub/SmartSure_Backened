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
        public async Task<IActionResult> GetDashboardMetrics()
        {
            var metrics = await _adminService.GetDashboardMetricsAsync();
            return Ok(metrics);
        }

        [HttpGet("users")]
        [EndpointSummary("[ADMIN ONLY] Get all users overview")]
        public async Task<IActionResult> GetAllUsersOverview()
        {
            var users = await _adminService.GetUserOverviewListAsync();
            return Ok(users);
        }

        [HttpGet("users/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Get user overview by ID")]
        public async Task<IActionResult> GetUserOverviewById(int id)
        {
            var user = await _adminService.GetUserOverviewByIdAsync(id);
            return Ok(user);
        }

        [HttpPut("users/{id:int}/status")]
        [EndpointSummary("[ADMIN ONLY] Update user status")]
        public async Task<IActionResult> UpdateUserStatus(int id, AdminUpdateUserStatusDto dto)
        {
            var updatedUser = await _adminService.UpdateUserStatusAsync(id, dto);
            return Ok(updatedUser);
        }

        [HttpGet("claims/unapproved")]
        [EndpointSummary("[ADMIN ONLY] Get all unapproved claims")]
        public async Task<IActionResult> GetUnapprovedClaims()
        {
            var claims = await _adminService.GetUnapprovedClaimsAsync();
            return Ok(claims);
        }
    }
}
