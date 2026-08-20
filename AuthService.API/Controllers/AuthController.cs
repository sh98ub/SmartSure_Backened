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
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _userService.RegisterAsync(dto);
            return StatusCode(201, result);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        [EndpointSummary("[PUBLIC] Authenticate user and issue JWT token")]
        public async Task<IActionResult> Login( LoginDto dto)
        {
            var result = await _userService.LoginAsync(dto);
            return Ok(result);
        }

        [HttpGet("admin/users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        [HttpGet("admin/users/{id:int}")]
        [Authorize(Roles = "Admin")]
        [EndpointSummary("[ADMIN ONLY] Get user details by ID")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }

        [HttpPut("admin/users/{id:int}/status")]
        [Authorize(Roles = "Admin")]
        [EndpointSummary("[ADMIN ONLY] Update user status & KYC")]
        public async Task<IActionResult> UpdateUserStatus(int id, UpdateUserStatusDto dto)
        {
            var updatedUser = await _userService.UpdateUserStatusAsync(id, dto);
            return Ok(updatedUser);
        }
    }
}
