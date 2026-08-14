using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ClaimService.Application.DTOs;
using ClaimService.Application.Interfaces;
using ClaimService.Domain;
using ClaimService.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ClaimService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimService.Infrastructure.Services
{
    public class ClaimProcessingService : IClaimProcessingService
    {
        private readonly ClaimDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _policyServiceUrl;

        public ClaimProcessingService(ClaimDbContext context, HttpClient? httpClient = null, IHttpContextAccessor? httpContextAccessor = null, IConfiguration? configuration = null)
        {
            _context = context;
            _httpClient = httpClient!;
            _httpContextAccessor = httpContextAccessor!;
            _policyServiceUrl = configuration?.GetSection("ServiceUrls")?["PolicyService"] ?? "http://localhost:5002";
        }

        public async Task<ClaimDto> SubmitClaimAsync(SubmitClaimDto dto)
        {
            if (dto.ClaimAmount <= 0)
            {
                throw new InvalidClaimAmountException(dto.ClaimAmount);
            }

            var incidentDate = dto.IncidentDate ?? DateTime.UtcNow;
            if (incidentDate > DateTime.UtcNow)
            {
                throw new ArgumentException("Incident date cannot be in the future.");
            }

            if (_httpClient != null)
            {
                // Sync HTTP communication: Validate user policy ownership with PolicyService
                var request = new HttpRequestMessage(HttpMethod.Get, $"{_policyServiceUrl}/api/policies/user/my-policies/{dto.UserPolicyId}");
                var authHeader = _httpContextAccessor?.HttpContext?.Request.Headers["Authorization"].ToString();
                
                if (!string.IsNullOrEmpty(authHeader))
                {
                    request.Headers.Add("Authorization", authHeader);
                }

                var response = await _httpClient.SendAsync(request);
                if (!response.IsSuccessStatusCode)
                {
                    throw new ArgumentException($"Failed to validate policy #{dto.UserPolicyId}. Either the policy does not exist, or you do not own it.");
                }
            }

            var claim = new Claim
            {
                UserPolicyId = dto.UserPolicyId,
                UserId = dto.UserId ?? 1,
                IncidentDate = incidentDate,
                ClaimAmount = dto.ClaimAmount,
                Description = dto.Description,
                SupportingDocumentUrl = dto.SupportingDocumentUrl,
                Status = ClaimStatus.Submitted,
                SubmittedAt = DateTime.UtcNow,
                Remarks = "Submitted successfully."
            };

            _context.Claims.Add(claim);
            await _context.SaveChangesAsync();

            return MapToDto(claim);
        }

        public async Task<IEnumerable<ClaimDto>> GetUserClaimsAsync(int userId)
        {
            var claims = await _context.Claims.Where(c => c.UserId == userId).ToListAsync();
            return claims.Select(MapToDto).ToList();
        }

        public async Task<IEnumerable<ClaimDto>> GetAllClaimsAsync()
        {
            var claims = await _context.Claims.ToListAsync();
            return claims.Select(MapToDto).ToList();
        }

        public async Task<ClaimDto?> GetClaimByIdAsync(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                throw new ClaimNotFoundException(id);
            }
            return MapToDto(claim);
        }

        public async Task<ClaimDto> ReviewClaimAsync(ReviewClaimDto dto)
        {
            var claim = await _context.Claims.FindAsync(dto.ClaimId);
            if (claim == null)
            {
                throw new ClaimNotFoundException(dto.ClaimId);
            }

            claim.Status = dto.Status;
            
            if (dto.Status == ClaimStatus.Approved)
            {
                claim.ApprovedPayoutAmount = dto.ApprovedPayoutAmount ?? claim.ClaimAmount;
                claim.Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? "Claim approved by administrator." : dto.Remarks;
            }
            else if (dto.Status == ClaimStatus.Rejected)
            {
                claim.ApprovedPayoutAmount = 0;
                claim.Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? "Claim rejected by administrator." : dto.Remarks;
            }
            else
            {
                claim.ApprovedPayoutAmount = dto.ApprovedPayoutAmount;
                claim.Remarks = dto.Remarks;
            }

            claim.ReviewedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return MapToDto(claim);
        }

        private static ClaimDto MapToDto(Claim claim)
        {
            return new ClaimDto
            {
                Id = claim.Id,
                ClaimNumber = "CLM-2026-" + claim.Id,
                UserPolicyId = claim.UserPolicyId,
                UserId = claim.UserId,
                IncidentDate = claim.IncidentDate,
                ClaimAmount = claim.ClaimAmount,
                Description = claim.Description,
                SupportingDocumentUrl = claim.SupportingDocumentUrl,
                Status = claim.Status.ToString(),
                Remarks = claim.Remarks,
                SubmittedAt = claim.SubmittedAt,
                ReviewedAt = claim.ReviewedAt,
                ApprovedPayoutAmount = claim.ApprovedPayoutAmount
            };
        }
    }
}
