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

        [HttpGet]
        [EndpointSummary("[ADMIN ONLY] Get all claims in system")]
        [EndpointDescription("Requires Admin role. Returns all claims filed across all users.")]
        public async Task<IActionResult> GetAllClaims()
        {
            var claims = await _claimService.GetAllClaimsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(claims, "All claims retrieved successfully."));
        }

        [HttpGet("unapproved")]
        [EndpointSummary("[ADMIN ONLY] Get unapproved claims")]
        [EndpointDescription("Requires Admin role. Returns all claims that are currently Submitted or UnderReview.")]
        public async Task<IActionResult> GetUnapprovedClaims()
        {
            var claims = await _claimService.GetUnapprovedClaimsAsync();
            return Ok(ApiResponse<object>.SuccessResponse(claims, "Unapproved claims retrieved successfully."));
        }

        [HttpGet("{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Get claim by ID for review")]
        [EndpointDescription("Requires Admin role. Retrieves detailed information for any claim in the system.")]
        public async Task<IActionResult> GetClaimById(int id)
        {
            var claim = await _claimService.GetClaimByIdAsync(id);
            if (claim == null) return NotFound(ApiResponse<string>.FailureResponse("Claim not found."));
            return Ok(ApiResponse<ClaimDto>.SuccessResponse(claim, "Claim retrieved successfully."));
        }

        [HttpPut("review")]
        [EndpointSummary("[ADMIN ONLY] Review and process claim")]
        [EndpointDescription("Requires Admin role. Approves or rejects a claim and sets the payout amount.")]
        public async Task<IActionResult> ReviewClaim([FromBody] ReviewClaimDto dto)
        {
            var claim = await _claimService.ReviewClaimAsync(dto);
            return Ok(ApiResponse<ClaimDto>.SuccessResponse(claim, "Claim review processed successfully."));
        }
    }
}
