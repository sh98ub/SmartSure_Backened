using System.Collections.Generic;
using System.Threading.Tasks;
using ClaimService.Domain;

namespace ClaimService.Application.Interfaces
{
    /// <summary>
    /// Repository interface for Claim entity persistence operations.
    /// Abstracts the underlying database queries for managing insurance claims.
    /// </summary>
    public interface IClaimRepository
    {
        /// <summary>
        /// Retrieves an insurance claim by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the claim.</param>
        /// <returns>A task representing the asynchronous operation, containing the Claim if found; otherwise, null.</returns>
        Task<Claim?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all insurance claims in the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a collection of all Claims.</returns>
        Task<IEnumerable<Claim>> GetAllAsync();

        /// <summary>
        /// Retrieves all claims filed by a specific user.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, containing a collection of Claims for the user.</returns>
        Task<IEnumerable<Claim>> GetByUserIdAsync(int userId);

        /// <summary>
        /// Retrieves all claims that are in the "Submitted" status and require administrator review.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a collection of unapproved Claims.</returns>
        Task<IEnumerable<Claim>> GetUnapprovedClaimsAsync();

        /// <summary>
        /// Adds a new claim to the repository context.
        /// </summary>
        /// <param name="claim">The claim entity to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(Claim claim);

        /// <summary>
        /// Persists all tracked changes to the underlying storage database.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveChangesAsync();
    }
}
