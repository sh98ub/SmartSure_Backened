using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PolicyService.Application.DTOs;
using PolicyService.Application.Interfaces;
using Shared.Models;

namespace PolicyService.APi.Controllers
{
    [ApiController]
    [Route("api/policies/user")]
    public class UserPoliciesController : ControllerBase
    {
        private readonly IPolicyManagementService _policyService;

        public UserPoliciesController(IPolicyManagementService policyService)
        {
            _policyService = policyService;
        }

        [HttpGet("plans")]
        [AllowAnonymous]
        [EndpointSummary("[PUBLIC] Get available policy plans")]
        [EndpointDescription("Returns list of active insurance policy plans available for subscription. Pick a policy plan ID here to buy.")]
        public async Task<IActionResult> GetPolicyPlans()
        {
            var plans = await _policyService.GetPolicyPlansAsync();
            return Ok(ApiResponse<object>.SuccessResponse(plans, "Available policy plans retrieved successfully. Choose a PolicyPlanId to subscribe."));
        }

        [HttpPost("subscribe")]
        [Authorize]
        [EndpointSummary("[USER] Subscribe / Buy a policy plan")]
        [EndpointDescription("Subscribes user to selected policy plan. Requires only the PolicyPlanId (chosen from GET /api/policies/user/plans). User ID is retrieved from JWT token.")]
        public async Task<IActionResult> Subscribe([FromBody] SubscribePolicyRequestDto dto)
        {
            var userId = GetCurrentUserId();
            var serviceDto = new SubscribePolicyDto
            {
                PolicyPlanId = dto.PolicyPlanId,
                UserId = userId,
                HasPreExistingConditions = dto.HasPreExistingConditions,
                IsSmoker = dto.IsSmoker,
                HasRecentHospitalization = dto.HasRecentHospitalization
            };

            var policy = await _policyService.SubscribePolicyAsync(serviceDto);
            return StatusCode(201, ApiResponse<UserPolicyDto>.SuccessResponse(policy, "Subscribed to policy plan successfully."));
        }

        [HttpGet("my-policies")]
        [Authorize]
        [EndpointSummary("[USER] View all my bought policies")]
        [EndpointDescription("Returns all active and historical policy subscriptions for the currently logged-in user. No ID required! Uses JWT token.")]
        public async Task<IActionResult> GetMyPolicies()
        {
            var userId = GetCurrentUserId();
            var policies = await _policyService.GetUserPoliciesAsync(userId);
            return Ok(ApiResponse<object>.SuccessResponse(policies, "Your bought policies retrieved successfully."));
        }

        [HttpGet("my-policies/{id:int}")]
        [Authorize]
        [EndpointSummary("[USER] Get specific policy subscription details")]
        [EndpointDescription("Retrieves details for a specific policy subscription by ID (e.g., 201). Users can only view their own policy.")]
        public async Task<IActionResult> GetMyPolicyById(int id)
        {
            var userId = GetCurrentUserId();
            var policy = await _policyService.GetUserPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound(ApiResponse<string>.FailureResponse("Policy subscription not found."));
            }

            if (policy.UserId != userId)
            {
                return StatusCode(403, ApiResponse<string>.FailureResponse("Access denied. You can only view your own policy details."));
            }

            return Ok(ApiResponse<UserPolicyDto>.SuccessResponse(policy, "Policy details retrieved successfully."));
        }

        [HttpPut("my-policies/{id:int}/cancel")]
        [Authorize]
        [EndpointSummary("[USER] Cancel active user policy")]
        [EndpointDescription("Cancels an active policy subscription. Users can only cancel their own policy.")]
        public async Task<IActionResult> CancelMyPolicy(int id)
        {
            var userId = GetCurrentUserId();
            var policy = await _policyService.GetUserPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound(ApiResponse<string>.FailureResponse("Policy subscription not found."));
            }

            if (policy.UserId != userId)
            {
                return StatusCode(403, ApiResponse<string>.FailureResponse("Access denied. You can only cancel your own policy."));
            }

            var success = await _policyService.CancelUserPolicyAsync(id);
            if (!success) return BadRequest(ApiResponse<string>.FailureResponse("Unable to cancel policy."));
            return Ok(ApiResponse<string>.SuccessResponse("Policy cancelled successfully.", "Policy cancelled successfully."));
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
