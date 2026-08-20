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
        public async Task<IActionResult> GetPolicyPlans()
        {
            var plans = await _policyService.GetPolicyPlansAsync();
            return Ok(plans);
        }

        [HttpGet("plans/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Get specific policy plan by ID")]
        public async Task<IActionResult> GetPolicyPlanById(int id)
        {
            var plan = await _policyService.GetPolicyPlanByIdAsync(id);
            if (plan == null) return NotFound($"Policy plan with ID '{id}' was not found.");
            return Ok(plan);
        }

        [HttpPost("plans")]
        [EndpointSummary("[ADMIN ONLY] Create new policy plan")]
        public async Task<IActionResult> CreatePolicyPlan(CreatePolicyPlanDto dto)
        {
            var plan = await _policyService.CreatePolicyPlanAsync(dto);
            return StatusCode(201, plan);
        }

        [HttpPut("plans/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Update existing policy plan")]
        public async Task<IActionResult> UpdatePolicyPlan(int id, UpdatePolicyPlanDto dto)
        {
            var plan = await _policyService.UpdatePolicyPlanAsync(id, dto);
            return Ok(plan);
        }

        [HttpDelete("plans/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] Delete/Deactivate a policy plan")]
        public async Task<IActionResult> DeletePolicyPlan(int id)
        {
            var success = await _policyService.DeletePolicyPlanAsync(id);
            if (!success) return BadRequest("Unable to delete policy plan.");
            return Ok("Policy plan deactivated successfully.");
        }

        [HttpGet("subscriptions")]
        [EndpointSummary("[ADMIN ONLY] View all user policy subscriptions")]
        public async Task<IActionResult> GetAllUserPolicies()
        {
            var policies = await _policyService.GetAllUserPoliciesAsync();
            return Ok(policies);
        }

        [HttpGet("subscriptions/{id:int}")]
        [EndpointSummary("[ADMIN ONLY] View specific policy subscription by ID")]
        public async Task<IActionResult> GetUserPolicyById(int id)
        {
            var policy = await _policyService.GetUserPolicyByIdAsync(id);
            if (policy == null) return NotFound("Policy subscription not found.");
            return Ok(policy);
        }
    }
}
