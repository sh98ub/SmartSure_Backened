using System;

namespace PolicyService.Domain.Exceptions
{
    public abstract class PolicyException : Exception
    {
        protected PolicyException(string message) : base(message) { }
    }

    public class PolicyPlanNotFoundException : PolicyException
    {
        public PolicyPlanNotFoundException(int planId)
            : base($"Policy plan with ID '{planId}' was not found in SmartSure Policy Catalog.") { }
    }

    public class UserPolicyNotFoundException : PolicyException
    {
        public UserPolicyNotFoundException(int policyId)
            : base($"User policy subscription with ID '{policyId}' was not found.") { }
    }

    public class PolicyAlreadyCancelledException : PolicyException
    {
        public PolicyAlreadyCancelledException(int policyId)
            : base($"User policy '{policyId}' has already been cancelled and cannot be modified.") { }
    }
}
