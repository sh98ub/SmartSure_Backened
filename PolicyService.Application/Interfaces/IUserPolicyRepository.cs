using System.Collections.Generic;
using System.Threading.Tasks;
using PolicyService.Domain;

namespace PolicyService.Application.Interfaces
{
    /// <summary>
    /// Repository interface for UserPolicy entity persistence operations.
    /// Abstracts database queries and management for user policy subscriptions.
    /// </summary>
    public interface IUserPolicyRepository
    {
        /// <summary>
        /// Retrieves a user policy subscription by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user policy.</param>
        /// <returns>A task representing the asynchronous operation, containing the UserPolicy if found; otherwise, null.</returns>
        Task<UserPolicy?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all policy subscriptions owned by a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of UserPolicies for the user.</returns>
        Task<IEnumerable<UserPolicy>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Retrieves all policy subscriptions in the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a collection of all UserPolicies.</returns>
        Task<IEnumerable<UserPolicy>> GetAllAsync();

        /// <summary>
        /// Adds a new user policy subscription to the repository context.
        /// </summary>
        /// <param name="policy">The user policy subscription entity to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(UserPolicy policy);

        /// <summary>
        /// Persists all tracked changes to the underlying storage database.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveChangesAsync();
    }
}
