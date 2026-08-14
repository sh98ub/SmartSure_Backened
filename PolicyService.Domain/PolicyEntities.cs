using System;

namespace PolicyService.Domain
{
    public class PolicyPlan
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public PolicyType Type { get; set; }
        public decimal BasePremium { get; set; }
        public decimal CoverageLimit { get; set; }
        public int DurationMonths { get; set; } = 12;
        public bool IsActive { get; set; } = true;
    }

    public class UserPolicy
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PolicyPlanId { get; set; }
        public decimal PremiumAmount { get; set; }
        public decimal CoverageLimit { get; set; }
        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public PolicyStatus Status { get; set; } = PolicyStatus.Active;
        public bool HasPreExistingConditions { get; set; }
        public bool IsSmoker { get; set; }
        public bool HasRecentHospitalization { get; set; }
    }
}
