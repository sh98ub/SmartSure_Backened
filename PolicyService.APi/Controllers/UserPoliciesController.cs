using System;
using System.Linq;
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
        public async Task<IActionResult> GetPolicyPlans()
        {
            var plans = await _policyService.GetPolicyPlansAsync();
            return Ok(plans);
        }

        [HttpPost("subscribe")]
        [Authorize]
        [EndpointSummary("[USER] Subscribe / Buy a policy plan")]
        public async Task<IActionResult> Subscribe(SubscribePolicyRequestDto dto)
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
            return StatusCode(201, policy);
        }

        [HttpGet("my-policies")]
        [Authorize]
        [EndpointSummary("[USER] View all my bought policies")]
        public async Task<IActionResult> GetMyPolicies()
        {
            var userId = GetCurrentUserId();
            var policies = await _policyService.GetUserPoliciesAsync(userId);
            var activePolicies = policies.Where(p => p.Status != "Cancelled");
            return Ok(activePolicies);
        }

        [HttpGet("my-policies/{id:int}")]
        [Authorize]
        [EndpointSummary("[USER] Get specific policy subscription details")]
        public async Task<IActionResult> GetMyPolicyById(int id)
        {
            var userId = GetCurrentUserId();
            var policy = await _policyService.GetUserPolicyByIdAsync(id);
            if (policy == null || policy.Status == "Cancelled")
            {
                return NotFound("Policy subscription not found.");
            }

            if (policy.UserId != userId)
            {
                return StatusCode(403, "Access denied. You can only view your own policy details.");
            }

            return Ok(policy);
        }

        [HttpPut("my-policies/{id:int}/cancel")]
        [Authorize]
        [EndpointSummary("[USER] Cancel active user policy")]
        public async Task<IActionResult> CancelMyPolicy(int id)
        {
            var userId = GetCurrentUserId();
            var policy = await _policyService.GetUserPolicyByIdAsync(id);
            if (policy == null)
            {
                return NotFound("Policy subscription not found.");
            }

            if (policy.UserId != userId)
            {
                return StatusCode(403, "Access denied. You can only cancel your own policy.");
            }

            var success = await _policyService.CancelUserPolicyAsync(id);
            if (!success) return BadRequest("Unable to cancel policy.");
            return Ok("Policy cancelled successfully.");
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
