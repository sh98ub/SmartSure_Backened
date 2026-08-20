using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AuthService.Application.Interfaces;
using AuthService.Domain;
using AuthService.Infrasturcture.Data;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrasturcture.Repositories
{
    /// <summary>
    /// EF Core implementation of the IUserRepository interface.
    /// Manages database access for user entities through the AuthDbContext.
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AuthDbContext _context;

        /// <summary>
        /// Initializes a new instance of the UserRepository class.
        /// </summary>
        /// <param name="context">The database context for AuthService database.</param>
        public UserRepository(AuthDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<User?> GetByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<User>> GetAllAsync()
        {
            return await _context.Users.ToListAsync();
        }

        /// <inheritdoc />
        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        /// <inheritdoc />
        public async Task<bool> ExistsByEmailOrUsernameAsync(string email, string username)
        {
            return await _context.Users.AnyAsync(u =>
                u.Email.ToLower() == email.ToLower() ||
                u.Username.ToLower() == username.ToLower());
        }

        /// <inheritdoc />
        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users.FirstOrDefaultAsync(u =>
                u.Username.ToLower() == usernameOrEmail.ToLower() ||
                u.Email.ToLower() == usernameOrEmail.ToLower());
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
