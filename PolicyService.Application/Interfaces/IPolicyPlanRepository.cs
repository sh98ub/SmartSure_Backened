using System.Collections.Generic;
using System.Threading.Tasks;
using PolicyService.Domain;

namespace PolicyService.Application.Interfaces
{
    /// <summary>
    /// Repository interface for PolicyPlan entity persistence operations.
    /// Abstracts database queries and management for available insurance plans.
    /// </summary>
    public interface IPolicyPlanRepository
    {
        /// <summary>
        /// Retrieves a policy plan by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the policy plan.</param>
        /// <returns>A task representing the asynchronous operation, containing the PolicyPlan if found; otherwise, null.</returns>
        Task<PolicyPlan?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all active policy plans.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a collection of active PolicyPlans.</returns>
        Task<IEnumerable<PolicyPlan>> GetActivePlansAsync();

        /// <summary>
        /// Retrieves policy type descriptions mapped by their unique IDs for the given list of plan IDs.
        /// </summary>
        /// <param name="planIds">The collection of unique policy plan IDs to fetch.</param>
        /// <returns>A task representing the asynchronous operation, containing a dictionary of PolicyPlanId mapped to PolicyType string.</returns>
        Task<Dictionary<int, string>> GetPlanTypesAsync(IEnumerable<int> planIds);

        /// <summary>
        /// Adds a new policy plan to the repository context.
        /// </summary>
        /// <param name="plan">The policy plan entity to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(PolicyPlan plan);

        /// <summary>
        /// Persists all tracked changes to the underlying storage database.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveChangesAsync();
    }
}
