using System;
using System.Security.Claims;
using System.Threading.Tasks;
using ClaimService.Application.DTOs;
using ClaimService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;

namespace ClaimService.API.Controllers
{
    [ApiController]
    [Route("api/claims/user")]
    [Authorize]
    public class UserClaimsController : ControllerBase
    {
        private readonly IClaimProcessingService _claimService;

        public UserClaimsController(IClaimProcessingService claimService)
        {
            _claimService = claimService;
        }

        [HttpPost]
        [EndpointSummary("[USER] Submit a new claim")]
        [EndpointDescription("Submits a new insurance claim for an active user policy. User ID is automatically retrieved from the JWT token.")]
        public async Task<IActionResult> SubmitClaim([FromBody] SubmitClaimDto dto)
        {
            var userId = GetCurrentUserId();
            dto.UserId = userId; // Force the logged-in user ID into the claim submission DTO

            var claim = await _claimService.SubmitClaimAsync(dto);
            return StatusCode(201, ApiResponse<ClaimDto>.SuccessResponse(claim, "Claim submitted successfully."));
        }

        [HttpGet("my-claims")]
        [EndpointSummary("[USER] View all my submitted claims")]
        [EndpointDescription("Retrieves all claims submitted by the currently logged-in user. No ID required! Uses JWT token.")]
        public async Task<IActionResult> GetMyClaims()
        {
            var userId = GetCurrentUserId();
            var claims = await _claimService.GetUserClaimsAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(claims, "Your submitted claims retrieved successfully."));
        }

        [HttpGet("my-claims/{id:int}")]
        [EndpointSummary("[USER] Get specific claim details")]
        [EndpointDescription("Retrieves details for a specific claim by ID (e.g. 301). Users can only view their own claims.")]
        public async Task<IActionResult> GetMyClaimById(int id)
        {
            var userId = GetCurrentUserId();
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
            {
                return NotFound(ApiResponse<string>.FailureResponse("Claim not found."));
            }

            if (claim.UserId != userId)
            {
                return StatusCode(403, ApiResponse<string>.FailureResponse("Access denied. You can only view your own claims."));
            }

            return Ok(ApiResponse<ClaimDto>.SuccessResponse(claim, "Claim details retrieved successfully."));
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return 1; // Fallback for testing without token
        }
    }
}
