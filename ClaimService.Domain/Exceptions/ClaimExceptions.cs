using System;

namespace ClaimService.Domain.Exceptions
{
    public abstract class ClaimException : Exception
    {
        protected ClaimException(string message) : base(message) { }
    }

    public class ClaimNotFoundException : ClaimException
    {
        public ClaimNotFoundException(int claimId)
            : base($"Claim with ID '{claimId}' was not found in SmartSure Claim Processing System.") { }
    }

    public class InvalidClaimAmountException : ClaimException
    {
        public InvalidClaimAmountException(decimal amount)
            : base($"Invalid claim submission: Claim amount ₹{amount} must be greater than zero.") { }
    }
}
