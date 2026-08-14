using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Interfaces;
using AuthService.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.API.Services
{
    public class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly string _secretKey;

        public JwtTokenGenerator(IConfiguration? configuration = null)
        {
            _secretKey = configuration?["Jwt:SecretKey"]
                ?? "SmartSureSecretKeyForJwtTokenGeneration2026!";
        }

        public string GenerateToken(User user, out DateTime expiresAt)
        {
            expiresAt = DateTime.UtcNow.AddHours(8);
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretKey);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.ToString())
                }),
                Expires = expiresAt,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
