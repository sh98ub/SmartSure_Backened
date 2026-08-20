using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PolicyService.Application.Interfaces;
using PolicyService.Domain;
using PolicyService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace PolicyService.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of the IUserPolicyRepository interface.
    /// Manages database access for UserPolicy subscriptions through the PolicyDbContext.
    /// </summary>
    public class UserPolicyRepository : IUserPolicyRepository
    {
        private readonly PolicyDbContext _context;

        /// <summary>
        /// Initializes a new instance of the UserPolicyRepository class.
        /// </summary>
        /// <param name="context">The database context for PolicyService database.</param>
        public UserPolicyRepository(PolicyDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<UserPolicy?> GetByIdAsync(int id)
        {
            return await _context.UserPolicies.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserPolicy>> GetByUserIdAsync(int userId)
        {
            return await _context.UserPolicies.Where(p => p.UserId == userId).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<UserPolicy>> GetAllAsync()
        {
            return await _context.UserPolicies.ToListAsync();
        }

        /// <inheritdoc />
        public async Task AddAsync(UserPolicy policy)
        {
            await _context.UserPolicies.AddAsync(policy);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
