using System.Collections.Generic;
using System.Threading.Tasks;
using AuthService.Domain;

namespace AuthService.Application.Interfaces
{
    /// <summary>
    /// Repository interface for User entity persistence operations.
    /// Provides abstraction over the database queries for AuthService users.
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// Retrieves a user by their unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the user.</param>
        /// <returns>A task representing the asynchronous operation, containing the User if found; otherwise, null.</returns>
        Task<User?> GetByIdAsync(int id);

        /// <summary>
        /// Retrieves all users in the system.
        /// </summary>
        /// <returns>A task representing the asynchronous operation, containing a collection of all Users.</returns>
        Task<IEnumerable<User>> GetAllAsync();

        /// <summary>
        /// Adds a new user to the repository context.
        /// </summary>
        /// <param name="user">The user entity to add.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddAsync(User user);

        /// <summary>
        /// Checks if a user already exists with the specified email or username.
        /// </summary>
        /// <param name="email">The email to check.</param>
        /// <param name="username">The username to check.</param>
        /// <returns>A task representing the asynchronous operation, containing true if exists; otherwise, false.</returns>
        Task<bool> ExistsByEmailOrUsernameAsync(string email, string username);

        /// <summary>
        /// Retrieves a user by their username or email address.
        /// </summary>
        /// <param name="usernameOrEmail">The username or email address of the user.</param>
        /// <returns>A task representing the asynchronous operation, containing the User if found; otherwise, null.</returns>
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);

        /// <summary>
        /// Persists all tracked changes to the underlying storage database.
        /// </summary>
        /// <returns>A task representing the asynchronous save operation.</returns>
        Task SaveChangesAsync();
    }
}
