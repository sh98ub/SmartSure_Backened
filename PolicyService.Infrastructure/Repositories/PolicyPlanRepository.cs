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
    /// EF Core implementation of the IPolicyPlanRepository interface.
    /// Manages database access for PolicyPlan entities through the PolicyDbContext.
    /// </summary>
    public class PolicyPlanRepository : IPolicyPlanRepository
    {
        private readonly PolicyDbContext _context;

        /// <summary>
        /// Initializes a new instance of the PolicyPlanRepository class.
        /// </summary>
        /// <param name="context">The database context for PolicyService database.</param>
        public PolicyPlanRepository(PolicyDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<PolicyPlan?> GetByIdAsync(int id)
        {
            return await _context.PolicyPlans.FindAsync(id);
        }

        /// <inheritdoc />
        public async Task<IEnumerable<PolicyPlan>> GetActivePlansAsync()
        {
            return await _context.PolicyPlans.Where(p => p.IsActive).ToListAsync();
        }

        /// <inheritdoc />
        public async Task<Dictionary<int, string>> GetPlanTypesAsync(IEnumerable<int> planIds)
        {
            return await _context.PolicyPlans
                .Where(p => planIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p.Type.ToString());
        }

        /// <inheritdoc />
        public async Task AddAsync(PolicyPlan plan)
        {
            await _context.PolicyPlans.AddAsync(plan);
        }

        /// <inheritdoc />
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
