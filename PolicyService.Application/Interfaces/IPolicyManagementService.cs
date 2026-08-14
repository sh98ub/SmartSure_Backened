using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PolicyService.Application.DTOs;

namespace PolicyService.Application.Interfaces
{
    public interface IPolicyManagementService
    {
        Task<IEnumerable<PolicyPlanDto>> GetPolicyPlansAsync();
        Task<PolicyPlanDto?> GetPolicyPlanByIdAsync(int id);
        Task<PolicyPlanDto> CreatePolicyPlanAsync(CreatePolicyPlanDto dto);
        Task<PolicyPlanDto> UpdatePolicyPlanAsync(int id, UpdatePolicyPlanDto dto);
        Task<bool> DeletePolicyPlanAsync(int id);
        Task<UserPolicyDto> SubscribePolicyAsync(SubscribePolicyDto dto);
        Task<IEnumerable<UserPolicyDto>> GetUserPoliciesAsync(int userId);
        Task<IEnumerable<UserPolicyDto>> GetAllUserPoliciesAsync();
        Task<UserPolicyDto?> GetUserPolicyByIdAsync(int id);
        Task<bool> CancelUserPolicyAsync(int id);
    }
}
