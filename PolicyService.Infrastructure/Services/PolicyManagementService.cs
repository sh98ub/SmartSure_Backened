using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolicyService.Application.DTOs;
using PolicyService.Application.Interfaces;
using PolicyService.Domain;
using PolicyService.Domain.Exceptions;
using PolicyService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PolicyService.Infrastructure.Services
{
    public class PolicyManagementService : IPolicyManagementService
    {
        private readonly PolicyDbContext _context;

        public PolicyManagementService(PolicyDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PolicyPlanDto>> GetPolicyPlansAsync()
        {
            var plans = await _context.PolicyPlans.Where(p => p.IsActive).ToListAsync();
            return plans.Select(MapToPlanDto).ToList();
        }

        public async Task<PolicyPlanDto?> GetPolicyPlanByIdAsync(int id)
        {
            var plan = await _context.PolicyPlans.FindAsync(id);
            if (plan == null)
            {
                throw new PolicyPlanNotFoundException(id);
            }
            return MapToPlanDto(plan);
        }

        public async Task<PolicyPlanDto> CreatePolicyPlanAsync(CreatePolicyPlanDto dto)
        {
            var plan = new PolicyPlan
            {
                Title = dto.Title,
                Description = dto.Description,
                Type = dto.Type,
                BasePremium = dto.BasePremium,
                CoverageLimit = dto.CoverageLimit,
                DurationMonths = dto.DurationMonths,
                IsActive = true
            };
            _context.PolicyPlans.Add(plan);
            await _context.SaveChangesAsync();
            return MapToPlanDto(plan);
        }

        public async Task<PolicyPlanDto> UpdatePolicyPlanAsync(int id, UpdatePolicyPlanDto dto)
        {
            var plan = await _context.PolicyPlans.FindAsync(id);
            if (plan == null)
            {
                throw new PolicyPlanNotFoundException(id);
            }

            plan.Title = dto.Title;
            plan.Description = dto.Description;
            plan.Type = dto.Type;
            plan.BasePremium = dto.BasePremium;
            plan.CoverageLimit = dto.CoverageLimit;
            plan.DurationMonths = dto.DurationMonths;
            plan.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            return MapToPlanDto(plan);
        }

        public async Task<bool> DeletePolicyPlanAsync(int id)
        {
            var plan = await _context.PolicyPlans.FindAsync(id);
            if (plan == null)
            {
                throw new PolicyPlanNotFoundException(id);
            }

            plan.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<UserPolicyDto> SubscribePolicyAsync(SubscribePolicyDto dto)
        {
            var plan = await _context.PolicyPlans.FindAsync(dto.PolicyPlanId);
            if (plan == null || !plan.IsActive)
            {
                throw new PolicyPlanNotFoundException(dto.PolicyPlanId);
            }

            var policy = new UserPolicy
            {
                UserId = dto.UserId,
                PolicyPlanId = plan.Id,
                PremiumAmount = plan.BasePremium,
                CoverageLimit = plan.CoverageLimit,
                StartDate = DateTime.UtcNow,
                EndDate = DateTime.UtcNow.AddMonths(plan.DurationMonths),
                Status = PolicyStatus.Active,
                HasPreExistingConditions = dto.HasPreExistingConditions,
                IsSmoker = dto.IsSmoker,
                HasRecentHospitalization = dto.HasRecentHospitalization
            };

            _context.UserPolicies.Add(policy);
            await _context.SaveChangesAsync();

            return MapToUserPolicyDto(policy, plan.Type.ToString());
        }

        public async Task<IEnumerable<UserPolicyDto>> GetUserPoliciesAsync(int userId)
        {
            var policies = await _context.UserPolicies.Where(p => p.UserId == userId).ToListAsync();
            var planIds = policies.Select(p => p.PolicyPlanId).Distinct().ToList();
            var plans = await _context.PolicyPlans.Where(p => planIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Type.ToString());

            return policies.Select(p => {
                plans.TryGetValue(p.PolicyPlanId, out var type);
                return MapToUserPolicyDto(p, type ?? string.Empty);
            }).ToList();
        }

        public async Task<IEnumerable<UserPolicyDto>> GetAllUserPoliciesAsync()
        {
            var policies = await _context.UserPolicies.ToListAsync();
            var planIds = policies.Select(p => p.PolicyPlanId).Distinct().ToList();
            var plans = await _context.PolicyPlans.Where(p => planIds.Contains(p.Id)).ToDictionaryAsync(p => p.Id, p => p.Type.ToString());

            return policies.Select(p => {
                plans.TryGetValue(p.PolicyPlanId, out var type);
                return MapToUserPolicyDto(p, type ?? string.Empty);
            }).ToList();
        }

        public async Task<UserPolicyDto?> GetUserPolicyByIdAsync(int id)
        {
            var policy = await _context.UserPolicies.FindAsync(id);
            if (policy == null)
            {
                throw new UserPolicyNotFoundException(id);
            }
            var plan = await _context.PolicyPlans.FindAsync(policy.PolicyPlanId);
            return MapToUserPolicyDto(policy, plan?.Type.ToString() ?? string.Empty);
        }

        public async Task<bool> CancelUserPolicyAsync(int id)
        {
            var policy = await _context.UserPolicies.FindAsync(id);
            if (policy == null)
            {
                throw new UserPolicyNotFoundException(id);
            }

            if (policy.Status == PolicyStatus.Cancelled)
            {
                throw new PolicyAlreadyCancelledException(id);
            }

            policy.Status = PolicyStatus.Cancelled;
            await _context.SaveChangesAsync();
            return true;
        }

        private static PolicyPlanDto MapToPlanDto(PolicyPlan plan)
        {
            return new PolicyPlanDto
            {
                Id = plan.Id,
                Title = plan.Title,
                Description = plan.Description,
                Type = plan.Type.ToString(),
                BasePremium = plan.BasePremium,
                CoverageLimit = plan.CoverageLimit,
                DurationMonths = plan.DurationMonths,
                IsActive = plan.IsActive
            };
        }

        private static UserPolicyDto MapToUserPolicyDto(UserPolicy policy, string type)
        {
            return new UserPolicyDto
            {
                Id = policy.Id,
                UserId = policy.UserId,
                PolicyPlanId = policy.PolicyPlanId,
                PolicyNumber = "POL-2026-" + policy.Id,
                Type = type,
                PremiumAmount = policy.PremiumAmount,
                CoverageLimit = policy.CoverageLimit,
                StartDate = policy.StartDate,
                EndDate = policy.EndDate,
                Status = policy.Status.ToString(),
                HasPreExistingConditions = policy.HasPreExistingConditions,
                IsSmoker = policy.IsSmoker,
                HasRecentHospitalization = policy.HasRecentHospitalization
            };
        }
    }
}
