using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimService.Application.DTOs;

namespace ClaimService.Application.Interfaces
{
    public interface IClaimProcessingService
    {
        Task<ClaimDto> SubmitClaimAsync(SubmitClaimDto dto);
        Task<IEnumerable<ClaimDto>> GetUserClaimsAsync(int userId);
        Task<IEnumerable<ClaimDto>> GetAllClaimsAsync();
        Task<ClaimDto?> GetClaimByIdAsync(int id);
        Task<ClaimDto> ReviewClaimAsync(ReviewClaimDto dto);
    }
}
