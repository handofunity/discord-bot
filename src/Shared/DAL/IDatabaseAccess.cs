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

        /// <summary>
        /// Gets all messages listed on the database.
        /// </summary>
        /// <returns>An array of tupples, containing the message name (key), a description and the current content.</returns>
        Task<(string Name, string Description, string Content)[]> GetAllMessages();

        /// <summary>
        /// Gets the current content for a message with a specific <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name (key) of the message.</param>
        /// <returns>The message content, if found, otherwise <b>null</b>.</returns>
        Task<string> GetMessageContent(string name);

        /// <summary>
        /// Sets the <paramref name="content"/> for a message with a specific <paramref name="name"/>.
        /// </summary>
        /// <param name="name">The name (key) of the message.</param>
        /// <param name="content">The new content of the message.</param>
        /// <returns><b>true</b>, if the <paramref name="name"/> exists, otherwise <b>false</b>.</returns>
        Task<bool> SetMessageContent(string name, string content);
    }
}