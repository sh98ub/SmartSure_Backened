using System;

namespace ClaimService.Domain
{
    public enum ClaimStatus
    {
        Submitted = 1,
        UnderReview = 2,
        Approved = 3,
        Rejected = 4,
        Paid = 5
    }

    public class Claim
    {
        public int Id { get; set; }
        public int UserPolicyId { get; set; }
        public int UserId { get; set; }
        public DateTime IncidentDate { get; set; }
        public decimal ClaimAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string SupportingDocumentUrl { get; set; } = string.Empty;
        public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
        public string Remarks { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public decimal? ApprovedPayoutAmount { get; set; }
    }
}
