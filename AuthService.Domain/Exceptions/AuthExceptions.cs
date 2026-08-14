using System;

namespace AuthService.Domain.Exceptions
{
    public abstract class AuthException : Exception
    {
        protected AuthException(string message) : base(message) { }
    }

    public class UserNotFoundException : AuthException
    {
        public UserNotFoundException(int userId)
            : base($"User with ID '{userId}' was not found in SmartSure Identity System.") { }

        public UserNotFoundException(string username)
            : base($"User with username '{username}' was not found in SmartSure Identity System.") { }
    }

    public class UserAlreadyExistsException : AuthException
    {
        public UserAlreadyExistsException(string username)
            : base($"User registration failed: Username '{username}' is already registered.") { }
    }

    public class InvalidCredentialsException : AuthException
    {
        public InvalidCredentialsException()
            : base("Authentication failed: Invalid username or password provided.") { }
    }
}
