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

namespace ClaimService.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for managing insurance claims, handling submission, validation, review, and query operations.
    /// Interacts with the repository layer instead of the DbContext directly.
    /// </summary>
    public class ClaimProcessingService : IClaimProcessingService
    {
        private readonly IClaimRepository _claimRepository;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _policyServiceUrl;

        /// <summary>
        /// Initializes a new instance of the ClaimProcessingService.
        /// </summary>
        /// <param name="claimRepository">The repository interface for claim persistence.</param>
        /// <param name="httpClient">Optional http client for downstream microservice calls.</param>
        /// <param name="httpContextAccessor">Optional accessor to forward caller tokens.</param>
        /// <param name="configuration">Optional configuration provider for microservice endpoints.</param>
        public ClaimProcessingService(
            IClaimRepository claimRepository, 
            HttpClient? httpClient = null, 
            IHttpContextAccessor? httpContextAccessor = null, 
            IConfiguration? configuration = null)
        {
            _claimRepository = claimRepository;
            _httpClient = httpClient!;
            _httpContextAccessor = httpContextAccessor!;
            _policyServiceUrl = configuration?.GetSection("ServiceUrls")?["PolicyService"] ?? "http://localhost:5002";
        }

      
      


        
        public class PolicyValidationDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public string Status { get; set; } = string.Empty;
        }

        /// <inheritdoc />
        public async Task<ClaimDto> SubmitClaimAsync(SubmitClaimDto dto, int userId)
        {
            // Enforce basic business rule validations
            if (dto.ClaimAmount <= 0)
            {
                throw new InvalidClaimAmountException(dto.ClaimAmount);
            }

            var incidentDate = DateTime.UtcNow;

            // Sync HTTP communication: Validate user policy ownership with PolicyService
            if (_httpClient != null)
            {
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

                using var responseStream = await response.Content.ReadAsStreamAsync();
                var policy = await System.Text.Json.JsonSerializer.DeserializeAsync<PolicyValidationDto>(
                    responseStream,
                    new System.Text.Json.JsonSerializerOptions { PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase });

                if (policy == null)
                {
                    throw new ArgumentException($"Failed to validate policy #{dto.UserPolicyId}. Either the policy does not exist, or you do not own it.");
                }

                // Check policy status (cannot submit claim on cancelled policy)
                if (string.Equals(policy.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    throw new PolicyAlreadyCancelledException(dto.UserPolicyId);
                }
            }

            // Construct and add Claim entity
            var claim = new Claim
            {
                UserPolicyId = dto.UserPolicyId,
                UserId = userId,
                IncidentDate = incidentDate,
                ClaimAmount = dto.ClaimAmount,
                Description = dto.Description,
                Status = "Submitted",
                SubmittedAt = DateTime.UtcNow,
                Remarks = "Submitted successfully."
            };

            await _claimRepository.AddAsync(claim);
            await _claimRepository.SaveChangesAsync();

            return MapToDto(claim);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ClaimDto>> GetUserClaimsAsync(int userId)
        {
            var claims = await _claimRepository.GetByUserIdAsync(userId);
            return claims.Select(MapToDto).ToList();
        }

        

        /// <inheritdoc />
        public async Task<IEnumerable<ClaimDto>> GetAllClaimsAsync()
        {
            var claims = await _claimRepository.GetAllAsync();
            return claims.Select(MapToDto).ToList();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<ClaimDto>> GetUnapprovedClaimsAsync()
        {
            var claims = await _claimRepository.GetUnapprovedClaimsAsync();
            return claims.Select(MapToDto).ToList();
        }

        /// <inheritdoc />
        public async Task<ClaimDto?> GetClaimByIdAsync(int id)
        {
            var claim = await _claimRepository.GetByIdAsync(id);
            if (claim == null)
            {
                throw new ClaimNotFoundException(id);
            }
            return MapToDto(claim);
        }

        /// <inheritdoc />
        public async Task<ClaimDto> ReviewClaimAsync(ReviewClaimDto dto)
        {
            var claim = await _claimRepository.GetByIdAsync(dto.ClaimId);
            if (claim == null)
            {
                throw new ClaimNotFoundException(dto.ClaimId);
            }

            claim.Status = dto.Status;
            
            // Adjust payouts and remarks according to claim decision
            if (string.Equals(dto.Status, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                claim.ApprovedPayoutAmount = dto.ApprovedPayoutAmount ?? claim.ClaimAmount;
                claim.Remarks = string.IsNullOrWhiteSpace(dto.Remarks) ? "Claim approved by administrator." : dto.Remarks;
            }
            else if (string.Equals(dto.Status, "Rejected", StringComparison.OrdinalIgnoreCase))
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

            await _claimRepository.SaveChangesAsync();

            return MapToDto(claim);
        }

        /// <summary>
        /// Helper mapping method to convert Domain Claim entity to Application ClaimDto representation.
        /// </summary>
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
                Status = claim.Status,
                Remarks = claim.Remarks,
                SubmittedAt = claim.SubmittedAt,
                ReviewedAt = claim.ReviewedAt,
                ApprovedPayoutAmount = claim.ApprovedPayoutAmount
            };
        }
    }
}
