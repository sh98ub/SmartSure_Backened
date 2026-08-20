using System;

namespace AdminService.Domain.Exceptions
{
    public abstract class AdminException : Exception
    {
        protected AdminException(string message) : base(message) { }
    }
}
