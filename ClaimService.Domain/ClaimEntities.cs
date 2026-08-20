using System;

namespace ClaimService.Domain
{
    public class Claim
    {
        public int Id { get; set; }
        public int UserPolicyId { get; set; }
        public int UserId { get; set; }
        public DateTime IncidentDate { get; set; }
        public decimal ClaimAmount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = "Submitted";
        public string Remarks { get; set; } = string.Empty;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReviewedAt { get; set; }
        public decimal? ApprovedPayoutAmount { get; set; }
    }
}
