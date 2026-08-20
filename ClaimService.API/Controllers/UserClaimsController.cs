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
    /// <summary>
    /// Handles claim operations submitted by policy holders.
    /// Requires user authentication.
    /// </summary>
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

        /// <summary>
        /// Submits a new claim against an active user policy.
        /// </summary>
        [HttpPost]
        [EndpointSummary("[USER] Submit a new claim")]
        public async Task<IActionResult> SubmitClaim(SubmitClaimDto dto)
        {
            var userId = GetCurrentUserId();
            var claim = await _claimService.SubmitClaimAsync(dto, userId);
            return StatusCode(201, claim);
        }
        
      
     

        /// <summary>
        /// Retrieves all claims filed by the currently logged-in user.
        /// </summary>
        [HttpGet("my-claims")]
        [EndpointSummary("[USER] View all my submitted claims")]
        public async Task<IActionResult> GetMyClaims()
        {
            var userId = GetCurrentUserId();
            var claims = await _claimService.GetUserClaimsAsync(userId);
            return Ok(claims);
        }
        

        /// <summary>
        /// Gets detailed information for a specific claim owned by the current user.
        /// </summary>
        [HttpGet("my-claims/{id:int}")]
        [EndpointSummary("[USER] Get specific claim details")]
        public async Task<IActionResult> GetMyClaimById(int id)
        {
            var userId = GetCurrentUserId();
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null)
            {
                return NotFound("Claim not found.");
            }

            // Restrict access to owner of the claim
            if (claim.UserId != userId)
            {
                return StatusCode(403, "Access denied. You can only view your own claims.");
            }

            return Ok(claim);
        }

        /// <summary>
        /// Extracts the user ID from the JWT authentication claims context.
        /// Defaults to 1 for developer testing without token.
        /// </summary>
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return 1;
        }
    }
}
