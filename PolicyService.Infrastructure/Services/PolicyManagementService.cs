using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolicyService.Application.DTOs;
using PolicyService.Application.Interfaces;
using PolicyService.Domain;
using PolicyService.Domain.Exceptions;

namespace PolicyService.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for managing policy plans and user policy subscriptions.
    /// Interacts with the repository layer instead of the DbContext directly.
    /// </summary>
    public class PolicyManagementService : IPolicyManagementService
    {
        private readonly IPolicyPlanRepository _policyPlanRepository;
        private readonly IUserPolicyRepository _userPolicyRepository;

        /// <summary>
        /// Initializes a new instance of the PolicyManagementService.
        /// </summary>
        /// <param name="policyPlanRepository">The repository interface for policy plan persistence.</param>
        /// <param name="userPolicyRepository">The repository interface for user policy subscription persistence.</param>
        public PolicyManagementService(
            IPolicyPlanRepository policyPlanRepository,
            IUserPolicyRepository userPolicyRepository)
        {
            _policyPlanRepository = policyPlanRepository;
            _userPolicyRepository = userPolicyRepository;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PolicyPlanDto>> GetPolicyPlansAsync()
        {
            var plans = await _policyPlanRepository.GetActivePlansAsync();
            return plans.Select(MapToPlanDto).ToList();
        }

        /// <inheritdoc />
        public async Task<PolicyPlanDto?> GetPolicyPlanByIdAsync(int id)
        {
            var plan = await _policyPlanRepository.GetByIdAsync(id);
            if (plan == null)
            {
                throw new PolicyPlanNotFoundException(id);
            }
            return MapToPlanDto(plan);
        }

        /// <inheritdoc />
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
            await _policyPlanRepository.AddAsync(plan);
            await _policyPlanRepository.SaveChangesAsync();
            return MapToPlanDto(plan);
        }

        /// <inheritdoc />
        public async Task<PolicyPlanDto> UpdatePolicyPlanAsync(int id, UpdatePolicyPlanDto dto)
        {
            var plan = await _policyPlanRepository.GetByIdAsync(id);
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

            await _policyPlanRepository.SaveChangesAsync();
            return MapToPlanDto(plan);
        }

        /// <inheritdoc />
        public async Task<bool> DeletePolicyPlanAsync(int id)
        {
            var plan = await _policyPlanRepository.GetByIdAsync(id);
            if (plan == null)
            {
                throw new PolicyPlanNotFoundException(id);
            }

            plan.IsActive = false; // Soft delete
            await _policyPlanRepository.SaveChangesAsync();
            return true;
        }

        /// <inheritdoc />
        public async Task<UserPolicyDto> SubscribePolicyAsync(SubscribePolicyDto dto)
        {
            var plan = await _policyPlanRepository.GetByIdAsync(dto.PolicyPlanId);
            if (plan == null || !plan.IsActive)
            {
                throw new PolicyPlanNotFoundException(dto.PolicyPlanId);
            }

            // Create new UserPolicy subscription, mapping attributes
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

            await _userPolicyRepository.AddAsync(policy);
            await _userPolicyRepository.SaveChangesAsync();

            return MapToUserPolicyDto(policy, plan.Type.ToString());
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserPolicyDto>> GetUserPoliciesAsync(int userId)
        {
            var policies = await _userPolicyRepository.GetByUserIdAsync(userId);
            var planIds = policies.Select(p => p.PolicyPlanId).Distinct().ToList();
            var plans = await _policyPlanRepository.GetPlanTypesAsync(planIds);

            return policies.Select(p => {
                plans.TryGetValue(p.PolicyPlanId, out var type);
                return MapToUserPolicyDto(p, type ?? string.Empty);
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserPolicyDto>> GetAllUserPoliciesAsync()
        {
            var policies = await _userPolicyRepository.GetAllAsync();
            var planIds = policies.Select(p => p.PolicyPlanId).Distinct().ToList();
            var plans = await _policyPlanRepository.GetPlanTypesAsync(planIds);

            return policies.Select(p => {
                plans.TryGetValue(p.PolicyPlanId, out var type);
                return MapToUserPolicyDto(p, type ?? string.Empty);
            }).ToList();
        }

        /// <inheritdoc />
        public async Task<UserPolicyDto?> GetUserPolicyByIdAsync(int id)
        {
            var policy = await _userPolicyRepository.GetByIdAsync(id);
            if (policy == null)
            {
                throw new UserPolicyNotFoundException(id);
            }
            var plan = await _policyPlanRepository.GetByIdAsync(policy.PolicyPlanId);
            return MapToUserPolicyDto(policy, plan?.Type.ToString() ?? string.Empty);
        }

        /// <inheritdoc />
        public async Task<bool> CancelUserPolicyAsync(int id)
        {
            var policy = await _userPolicyRepository.GetByIdAsync(id);
            if (policy == null)
            {
                throw new UserPolicyNotFoundException(id);
            }

            if (policy.Status == PolicyStatus.Cancelled)
            {
                throw new PolicyAlreadyCancelledException(id);
            }

            policy.Status = PolicyStatus.Cancelled;
            await _userPolicyRepository.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Helper mapping method to convert Domain PolicyPlan entity to Application PolicyPlanDto representation.
        /// </summary>
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

        /// <summary>
        /// Helper mapping method to convert Domain UserPolicy entity to Application UserPolicyDto representation.
        /// </summary>
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
