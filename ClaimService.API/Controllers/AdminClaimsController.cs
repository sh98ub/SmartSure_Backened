using System;
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
    /// Handles administrative review and tracking of system claims.
    /// Requires Admin role.
    /// </summary>
    [ApiController]
    [Route("api/claims/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminClaimsController : ControllerBase
    {
        private readonly IClaimProcessingService _claimService;

        public AdminClaimsController(IClaimProcessingService claimService)
        {
            _claimService = claimService;
        }

        /// <summary>
        /// Retrieves all claims across all users in the system.
        /// </summary>
        [HttpGet]
        [EndpointSummary("[ADMIN ONLY] Get all claims in system")]
        public async Task<IActionResult> GetAllClaims()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return Ok(claims);
        }

        /// <summary>
        /// Retrieves all unapproved (Submitted) claims.
        /// </summary>
        [HttpGet("unapproved")]
        [EndpointSummary("[ADMIN ONLY] Get unapproved claims")]
        public async Task<IActionResult> GetUnapprovedClaims()
        {
            var claims = await _claimService.GetUnapprovedClaimsAsync();
            return Ok(claims);
        }

        /// <summary>
        /// Retrieves a specific claim by ID for administrator review.
        /// </summary>
        [HttpGet("{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Get claim by ID for review")]
        public async Task<IActionResult> GetClaimById(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null) return NotFound("Claim not found.");
            return Ok(claim);
        }

        /// <summary>
        /// Processes a claim review, approving or rejecting and allocating payouts.
        /// </summary>
        [HttpPut("review")]
        [EndpointSummary("[ADMIN ONLY] Review and process claim")]
        public async Task<IActionResult> ReviewClaim(ReviewClaimDto dto)
        {
            var claim = await _claimService.ReviewClaimAsync(dto);
            return Ok(claim);
        }
    }
}
