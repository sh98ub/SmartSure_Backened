using System;
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
    [Route("api/policies/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminPoliciesController : ControllerBase
    {
        private readonly IPolicyManagementService _policyService;

        public AdminPoliciesController(IPolicyManagementService policyService)
        {
            _policyService = policyService;
        }

        [HttpGet("plans")]
        [EndpointSummary("[ADMIN ONLY] View all policy plans")]
        [EndpointDescription("Requires Admin role. Returns all active and inactive policy plans in catalog.")]
        public async Task<IActionResult> GetPolicyPlans()
        {
            var plans = await _policyService.GetPolicyPlansAsync();
            return Ok(ApiResponse<object>.SuccessResponse(plans, "Policy plans retrieved successfully."));
        }

        [HttpGet("plans/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Get specific policy plan by ID")]
        [EndpointDescription("Requires Admin role. Returns details for a specific policy plan by ID.")]
        public async Task<IActionResult> GetPolicyPlanById(int id)
        {
            var plan = await _policyService.GetPolicyPlanByIdAsync(id);
            if (plan == null) return NotFound(ApiResponse<string>.FailureResponse($"Policy plan with ID '{id}' was not found."));
            return Ok(ApiResponse<PolicyPlanDto>.SuccessResponse(plan, "Policy plan retrieved successfully."));
        }

        [HttpPost("plans")]
        [EndpointSummary("[ADMIN ONLY] Create new policy plan")]
        [EndpointDescription("Requires Admin role. Creates a new insurance policy plan in system.")]
        public async Task<IActionResult> CreatePolicyPlan([FromBody] CreatePolicyPlanDto dto)
        {
            var plan = await _policyService.CreatePolicyPlanAsync(dto);
            return StatusCode(201, ApiResponse<PolicyPlanDto>.SuccessResponse(plan, "Policy plan created successfully."));
        }

        [HttpPut("plans/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Update existing policy plan")]
        [EndpointDescription("Requires Admin role. Updates title, premium, limit, duration, or active status of a policy plan.")]
        public async Task<IActionResult> UpdatePolicyPlan(int id, [FromBody] UpdatePolicyPlanDto dto)
        {
            var plan = await _policyService.UpdatePolicyPlanAsync(id, dto);
            return Ok(ApiResponse<PolicyPlanDto>.SuccessResponse(plan, "Policy plan updated successfully."));
        }

        [HttpDelete("plans/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Delete/Deactivate a policy plan")]
        [EndpointDescription("Requires Admin role. Soft-deletes (deactivates) a policy plan by ID.")]
        public async Task<IActionResult> DeletePolicyPlan(int id)
        {
            var success = await _policyService.DeletePolicyPlanAsync(id);
            if (!success) return BadRequest(ApiResponse<string>.FailureResponse("Unable to delete policy plan."));
            return Ok(ApiResponse<string>.SuccessResponse("Policy plan deactivated successfully.", "Policy plan deactivated successfully."));
        }

        [HttpGet("subscriptions")]
        [EndpointSummary("[ADMIN ONLY] View all user policy subscriptions")]
        [EndpointDescription("Requires Admin role. Returns all active policy subscriptions system-wide.")]
        public async Task<IActionResult> GetAllUserPolicies()
        {
            var policies = await _policyService.GetAllUserPoliciesAsync();
            return Ok(ApiResponse<object>.SuccessResponse(policies, "All user policies retrieved successfully."));
        }

        [HttpGet("subscriptions/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] View specific policy subscription by ID")]
        [EndpointDescription("Requires Admin role. Returns details for any user policy subscription.")]
        public async Task<IActionResult> GetUserPolicyById(int id)
        {
            var policy = await _policyService.GetUserPolicyByIdAsync(id);
            if (policy == null) return NotFound(ApiResponse<string>.FailureResponse("Policy subscription not found."));
            return Ok(ApiResponse<UserPolicyDto>.SuccessResponse(policy, "User policy retrieved successfully."));
        }
    }
}
