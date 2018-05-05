namespace HoU.GuildBot.Shared.DAL
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IDatabaseAccess
    {
        /// <summary>
        /// Adds a given set of user IDs to the database.
        /// </summary>
        /// <param name="userIDs">The IDs to add.</param> 
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        /// <remarks>User IDs that are already present on the database won't be added.</remarks>
        Task AddUsers(IEnumerable<ulong> userIDs);

        /// <summary>
        /// Adds a user ID to the database.
        /// </summary>
        /// <param name="userID">The ID to add.</param>
        /// <returns><b>True</b>, if the user was added, otherwise <b>false</b>.</returns>
        /// <remarks>If the user ID is already present on the database, it won't be added and the return value will be false.</remarks>
        Task<bool> AddUser(ulong userID);
    }
}