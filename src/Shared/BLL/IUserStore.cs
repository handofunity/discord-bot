namespace HoU.GuildBot.Shared.BLL
{
    using System;
    using System.Threading.Tasks;
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
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Initialize();

        /// <summary>
        /// Gets an user by the <see cref="DiscordUserID"/>.
        /// </summary>
        /// <param name="userID">The external Discord user ID.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        /// <returns>The <see cref="User"/> object.</returns>
        /// <remarks>If the store doesn't know any user with the <paramref name="userID"/>, the object is added to the store and database.</remarks>
        Task<User> GetUser(DiscordUserID userID);

        /// <summary>
        /// Gets an user by the <see cref="InternalUserID"/>.
        /// </summary>
        /// <param name="userID">The internal user ID.</param>
        /// <exception cref="InvalidOperationException">The store is not initialized.</exception>
        /// <exception cref="ArgumentOutOfRangeException">No user could be found for the given <paramref name="userID"/>.</exception>
        /// <returns>The <see cref="User"/> object.</returns>
        User GetUser(InternalUserID userID);
    }
}