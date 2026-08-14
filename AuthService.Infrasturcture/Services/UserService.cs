using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain;
using AuthService.Domain.Exceptions;
using AuthService.Infrasturcture.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrasturcture.Services
{
    public class UserService : IUserService
    {
        private readonly AuthDbContext _context;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public UserService(AuthDbContext context, IJwtTokenGenerator jwtTokenGenerator)
        {
            _context = context;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u =>
                u.Email.ToLower() == dto.Email.ToLower() ||
                u.Username.ToLower() == dto.Email.ToLower()))
            {
                throw new UserAlreadyExistsException(dto.Email);
            }

            var user = new User
            {
                Username = dto.Email,   // use email as username
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
                FullName = dto.Name,
                KycStatus = "Pending",
                Role = UserRole.PolicyHolder,  // always force PolicyHolder — never trust user input
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RegisterResponseDto
            {
                Message = "Registration successful. Please login to get your access token.",
                User = MapToDto(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                u.Username.ToLower() == dto.Username.ToLower() ||
                u.Email.ToLower() == dto.Username.ToLower());

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            var token = _jwtTokenGenerator.GenerateToken(user, out var expiresAt);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToDto(user),
                ExpiresAt = expiresAt
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException(id);
            }
            return MapToDto(user);
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _context.Users.ToListAsync();
            return users.Select(MapToDto).ToList();
        }

        public async Task<UserDto> UpdateUserStatusAsync(int id, UpdateUserStatusDto dto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException(id);
            }

            user.IsActive = dto.IsActive;
            if (!string.IsNullOrWhiteSpace(dto.KycStatus))
            {
                user.KycStatus = dto.KycStatus;
            }

            await _context.SaveChangesAsync();

            return MapToDto(user);
        }

        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                KycStatus = user.KycStatus,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
