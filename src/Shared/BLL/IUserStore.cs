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
        /// <param name="user">The matching user object, or <b>null</b> if none could have been found.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        /// <returns>The <see cref="User"/> object.</returns>
        bool TryGetUser(DiscordUserID userID, out User user);

        /// <summary>
        /// Gets an user by the <see cref="InternalUserID"/>.
        /// </summary>
        /// <param name="userID">The internal user ID.</param>
        /// <param name="user">The matching user object, or <b>null</b> if none could have been found.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        /// <returns>The <see cref="User"/> object.</returns>
        bool TryGetUser(InternalUserID userID, out User user);

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
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        /// <remarks>If the store doesn't know any user with the <paramref name="userID"/>,
        /// the object is added to the store and database.</remarks>
        Task AddUserIfNewAsync(DiscordUserID userID, Role roles);

        /// <summary>
        /// Removes the user by the <see cref="DiscordUserID"/>.
        /// </summary>
        /// <param name="userID">The ID of the user to remove.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        Task RemoveUser(DiscordUserID userID);
    }
}