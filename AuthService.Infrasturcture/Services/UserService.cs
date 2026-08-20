using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Application.DTOs;
using AuthService.Application.Interfaces;
using AuthService.Domain;
using AuthService.Domain.Exceptions;

namespace AuthService.Infrasturcture.Services
{
    /// <summary>
    /// Service implementation for managing users, handling registration, login, status updates, and retrieval.
    /// Interacts with the repository layer instead of the DbContext directly.
    /// </summary>
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        /// <summary>
        /// Initializes a new instance of the UserService.
        /// </summary>
        /// <param name="userRepository">The abstract repository for user persistence.</param>
        /// <param name="jwtTokenGenerator">The JWT token generator utility.</param>
        public UserService(IUserRepository userRepository, IJwtTokenGenerator jwtTokenGenerator)
        {
            _userRepository = userRepository;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        /// <inheritdoc />
        public async Task<RegisterResponseDto> RegisterAsync(RegisterDto dto)
        {
            // Verify if email or username is already registered in the system
            if (await _userRepository.ExistsByEmailOrUsernameAsync(dto.Email, dto.Email))
            {
                throw new UserAlreadyExistsException(dto.Email);
            }

            // Create new User entity, hashing the password using BCrypt
            var user = new User
            {
                Username = dto.Email,   // use email as username
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password, workFactor: 12),
                FullName = dto.Name,
                Role = UserRole.PolicyHolder,  // always force PolicyHolder — never trust user input
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();

            return new RegisterResponseDto
            {
                Message = "Registration successful. Please login to get your access token.",
                User = MapToDto(user)
            };
        }

        /// <inheritdoc />
        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            // Retrieve user by username or email
            var user = await _userRepository.GetByUsernameOrEmailAsync(dto.Username);

            // Verify credentials with BCrypt verification
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new InvalidCredentialsException();
            }

            // Generate JWT token for successful login
            var token = _jwtTokenGenerator.GenerateToken(user, out var expiresAt);

            return new AuthResponseDto
            {
                Token = token
            };
        }

        /// <inheritdoc />
        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException(id);
            }
            return MapToDto(user);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users.Select(MapToDto).ToList();
        }

        /// <inheritdoc />
        public async Task<UserDto> UpdateUserStatusAsync(int id, UpdateUserStatusDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                throw new UserNotFoundException(id);
            }

            // Toggle active state
            user.IsActive = dto.IsActive;

            await _userRepository.SaveChangesAsync();

            return MapToDto(user);
        }

        /// <summary>
        /// Helper mapping method to convert Domain User entity to Application UserDto representation.
        /// </summary>
        private static UserDto MapToDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Role = user.Role.ToString(),
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };
        }
    }
}
