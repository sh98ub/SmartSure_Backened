using System;

namespace AdminService.Domain.Exceptions
{
    public abstract class AdminException : Exception
    {
        protected AdminException(string message) : base(message) { }
    }

    public class AuditLogNotFoundException : AdminException
    {
        public AuditLogNotFoundException(int logId)
            : base($"Audit log entry with ID '{logId}' was not found in SmartSure Telemetry System.") { }
    }
}
