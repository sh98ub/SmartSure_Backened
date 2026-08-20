using System;
using System.ComponentModel.DataAnnotations;
using ClaimService.Domain;

namespace ClaimService.Application.DTOs
{
    public class SubmitClaimDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Valid UserPolicyId is required.")]
        public int UserPolicyId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Claim amount must be greater than zero.")]
        public decimal ClaimAmount { get; set; }

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;
    }

    public class ReviewClaimDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Valid ClaimId is required.")]
        public int ClaimId { get; set; }

        [Required(ErrorMessage = "Claim status is required.")]
        public string Status { get; set; } = string.Empty;

        [Range(0.0, double.MaxValue, ErrorMessage = "Approved payout amount cannot be negative.")]
        public decimal? ApprovedPayoutAmount { get; set; }

        [StringLength(500, ErrorMessage = "Remarks cannot exceed 500 characters.")]
        public string Remarks { get; set; } = string.Empty;
    }

    public class ClaimDto
    {
        public int Id { get; set; }
        public string ClaimNumber { get; set; } = string.Empty;
        public int UserPolicyId { get; set; }
        public int UserId { get; set; }
        public DateTime IncidentDate { get; set; }
        public decimal ClaimAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public decimal? ApprovedPayoutAmount { get; set; }
    }

    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal salary { get; set; }
    }
}
