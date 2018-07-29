namespace HoU.GuildBot.Shared.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Enums;
    using Objects;
    using StrongTypes;

    public interface IUserStore
    {
        /// <summary>
        /// Gets if the user store is already initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Loads the store.
        /// </summary>
        /// <param name="guildUsers">User information provided by Discord.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Initialize((DiscordUserID UserId, Role Roles)[] guildUsers);

        /// <summary>
        /// Gets an user by the <see cref="DiscordUserID"/>.
        /// </summary>
        /// <param name="userID">The external Discord user ID.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        /// <exception cref="KeyNotFoundException">No user could be found for the given <paramref name="userID"/>.</exception>
        /// <returns>The <see cref="User"/> object.</returns>
        User GetUser(DiscordUserID userID);

        /// <summary>
        /// Gets an user by the <see cref="InternalUserID"/>.
        /// </summary>
        /// <param name="userID">The internal user ID.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        /// <exception cref="KeyNotFoundException">No user could be found for the given <paramref name="userID"/>.</exception>
        /// <returns>The <see cref="User"/> object.</returns>
        User GetUser(InternalUserID userID);

        /// <summary>
        /// Gets all users matching the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">The predicate to match the user objects against.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        /// <returns>All matching entries.</returns>
        User[] GetUsers(Predicate<User> predicate);

        /// <summary>
        /// Gets an user by the <see cref="DiscordUserID"/> and the state, if it is a new user or an existing user.
        /// </summary>
        /// <param name="userID">The external Discord user ID.</param>
        /// <param name="roles">The current roles of the user.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        /// <returns>A tupple containing the <see cref="User"/> object and the state, if it is a new user or an existing user.</returns>
        /// <remarks>If the store doesn't know any user with the <paramref name="userID"/>,
        /// the object is added to the store and database, and <b>IsNew</b> will be <b>true</b>.</remarks>
        Task<(User User, bool IsNew)> GetOrAddUser(DiscordUserID userID, Role roles);

        /// <summary>
        /// Removes the user by the <see cref="DiscordUserID"/>.
        /// </summary>
        /// <param name="userID">The ID of the user to remove.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        Task RemoveUser(DiscordUserID userID);
    }
}