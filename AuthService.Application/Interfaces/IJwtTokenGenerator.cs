using System;
using AuthService.Domain;

namespace AuthService.Application.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateToken(User user, out DateTime expiresAt);
    }
}
