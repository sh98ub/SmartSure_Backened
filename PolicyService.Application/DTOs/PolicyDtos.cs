using System;
using System.ComponentModel.DataAnnotations;
using PolicyService.Domain;

namespace PolicyService.Application.DTOs
{
    public class PolicyPlanDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal BasePremium { get; set; }
        public decimal CoverageLimit { get; set; }
        public int DurationMonths { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreatePolicyPlanDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Policy type is required.")]
        public PolicyType Type { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Base premium must be greater than zero.")]
        public decimal BasePremium { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Coverage limit must be greater than zero.")]
        public decimal CoverageLimit { get; set; }

        [Range(1, 120, ErrorMessage = "Duration must be between 1 and 120 months.")]
        public int DurationMonths { get; set; } = 12;
    }

    public class UpdatePolicyPlanDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Policy type is required.")]
        public PolicyType Type { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Base premium must be greater than zero.")]
        public decimal BasePremium { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Coverage limit must be greater than zero.")]
        public decimal CoverageLimit { get; set; }

        [Range(1, 120, ErrorMessage = "Duration must be between 1 and 120 months.")]
        public int DurationMonths { get; set; } = 12;

        public bool IsActive { get; set; } = true;
    }

    public class SubscribePolicyRequestDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "Valid PolicyPlanId is required.")]
        public int PolicyPlanId { get; set; }

        public bool HasPreExistingConditions { get; set; }
        public bool IsSmoker { get; set; }
        public bool HasRecentHospitalization { get; set; }
    }

    public class SubscribePolicyDto
    {
        public int UserId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Valid PolicyPlanId is required.")]
        public int PolicyPlanId { get; set; }

        public bool HasPreExistingConditions { get; set; }
        public bool IsSmoker { get; set; }
        public bool HasRecentHospitalization { get; set; }
    }

    public class UserPolicyDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PolicyPlanId { get; set; }
        public string PolicyNumber { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal PremiumAmount { get; set; }
        public decimal CoverageLimit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Status { get; set; } = string.Empty;

        public bool HasPreExistingConditions { get; set; }
        public bool IsSmoker { get; set; }
        public bool HasRecentHospitalization { get; set; }
    }
}
