using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ClaimService.Application.Interfaces;
using ClaimService.Domain;
using ClaimService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ClaimService.Infrastructure.Repositories
{
    /// <summary>
    /// EF Core implementation of the IClaimRepository interface.
    /// Manages database access for Claim entities through the ClaimDbContext.
    /// </summary>
    public class ClaimRepository : IClaimRepository
    {
        private readonly ClaimDbContext _context;

        /// <summary>
        /// Initializes a new instance of the ClaimRepository class.
        /// </summary>
        /// <param name="context">The database context for ClaimService database.</param>
        public ClaimRepository(ClaimDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<Claim?> GetByIdAsync(int id)
        {
            return await _context.Claims.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Claim>> GetAllAsync()
        {
            return await _context.Claims.ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Claim>> GetByUserIdAsync(int userId)
        {
            return await _context.Claims.Where(c => c.UserId == userId).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<IEnumerable<Claim>> GetUnapprovedClaimsAsync()
        {
            return await _context.Claims.Where(c => c.Status == "Submitted").ToListAsync();
        }

        /// <inheritdoc />
        public async Task AddAsync(Claim claim)
        {
            await _context.Claims.AddAsync(claim);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
