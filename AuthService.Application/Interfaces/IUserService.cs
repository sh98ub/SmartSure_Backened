using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.Application.DTOs;

namespace AuthService.Application.Interfaces
{
    public interface IUserService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto> UpdateUserStatusAsync(int id, UpdateUserStatusDto dto);
    }
}
