using System;
using System.Threading.Tasks;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        [EndpointSummary("[PUBLIC] Register a new user account")]
        [EndpointDescription("Registers a new user with name, email, and password. Default role is PolicyHolder. No token is issued on registration — use /login to obtain a token.")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            return StatusCode(201, ApiResponse<RegisterResponseDto>.SuccessResponse(result, "User registered successfully."));
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EndpointSummary("[PUBLIC] Authenticate user and issue JWT token")]
        [EndpointDescription("Authenticates user credentials and returns JWT bearer token.")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            return Ok(ApiResponse<AuthResponseDto>.SuccessResponse(result, "Login successful."));
        }

        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        [EndpointSummary("[ADMIN ONLY] Get all registered users")]
        [EndpointDescription("Requires Admin role. Returns list of all registered users in the system.")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(ApiResponse<object>.SuccessResponse(users, "Users retrieved successfully."));
        }

        [HttpGet("admin/users/{id:int}")]
        [Authorize(Roles = "Admin")]
        [EndpointSummary("[ADMIN ONLY] Get user details by ID")]
        [EndpointDescription("Requires Admin role. Returns full user profile details for a specific user ID.")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(ApiResponse<UserDto?>.SuccessResponse(user, "User details retrieved successfully."));
        }

        [HttpPut("admin/users/{id:int}/status")]
        [Authorize(Roles = "Admin")]
        [EndpointSummary("[ADMIN ONLY] Update user status & KYC")]
        [EndpointDescription("Requires Admin role. Updates account active status or KYC verification status.")]
        public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
        {
            var updatedUser = await _userService.UpdateUserStatusAsync(id, dto);
            return Ok(ApiResponse<UserDto>.SuccessResponse(updatedUser, "User status updated successfully."));
        }
    }
}
