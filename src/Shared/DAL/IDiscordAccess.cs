namespace HoU.GuildBot.Shared.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Objects;
    using StrongTypes;

    public interface IDiscordAccess
    {
        /// <summary>
        /// Gets if the Discord access is connected to Discord.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Tries to establish a connection to Discord.
        /// </summary>
        /// <param name="connectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection has been established.</param>
        /// <param name="disconnectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection is lost.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectedHandler"/> is <b>null</b>.</exception>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Connect(Func<Task> connectedHandler, Func<Task> disconnectedHandler);

        /// <summary>
        /// Sets the <paramref name="gameName"/> as current bot game name.
        /// </summary>
        /// <param name="gameName">The game name.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SetCurrentGame(string gameName);

        /// <summary>
        /// Logs the <paramref name="message"/> in the dedicated logging channel on Discord.
        /// </summary>
        /// <param name="message">The message to log.</param>
        /// <exception cref="ArgumentNullException"><paramref name="message"/> is <b>null</b>.</exception>
        /// <exception cref="ArgumentException"><paramref name="message"/> is empty or only whitespaces.</exception>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task LogToDiscord(string message);

        /// <summary>
        /// Checks if the user with the <paramref name="userID"/> is currently online.
        /// </summary>
        /// <param name="userID">The ID of the user to check.</param>
        /// <returns><b>True</b>, if the user is online, otherwise <b>false</b>.</returns>
        bool IsUserOnline(DiscordUserID userID);

        /// <summary>
        /// Gets the user names for the given <paramref name="userIDs"/>.
        /// </summary>
        /// <param name="userIDs">The user IDs to get the names for.</param>
        /// <returns>A mapping from a userID to a display name.</returns>
        Dictionary<DiscordUserID, string> GetUserNames(IEnumerable<DiscordUserID> userIDs);

        /// <summary>
        /// Gets the available class names for the <paramref name="game"/>.
        /// </summary>
        /// <param name="game">The game to get the class names for.</param>
        /// <exception cref="ArgumentNullException"><paramref name="game"/> is <b>null</b>.</exception>
        void GetClassNamesForGame(AvailableGame game);

        /// <summary>
        /// Revokes all current roles related with the <paramref name="userID"/> for the given <paramref name="game"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="game">The game.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task RevokeCurrentGameRoles(DiscordUserID userID, AvailableGame game);

        /// <summary>
        /// Sets the current role related with the <paramref name="userID"/> for the given <paramref name="game"/> to a specific <paramref name="className"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="game">The game.</param>
        /// <param name="className">The <paramref name="game"/> related class name.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SetCurrentGameRole(DiscordUserID userID, AvailableGame game, string className);

        /// <summary>
        /// Checks if the bot can manage roles for a specific <paramref name="userID"/>, depending on the guilds role configuration.
        /// </summary>
        /// <param name="userID">The ID of the user to change roles for.</param>
        /// <returns><b>True</b>, if the bot can manage roles for the user, otherwise <b>false</b>.</returns>
        bool CanManageRolesForUser(DiscordUserID userID);

        /// <summary>
        /// Sends the welcome message to new people joining the server.
        /// </summary>
        /// <param name="userID">The <see cref="DiscordUserID"/> to send the welcome message to.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendWelcomeMessage(DiscordUserID userID);

        /// <summary>
        /// Gets the mention string for the give <paramref name="roleName"/>.
        /// </summary>
        /// <param name="roleName">The name of the role to get the mention for.</param>
        /// <exception cref="InvalidOperationException">No role with the given <paramref name="roleName"/> exists.</exception>
        /// <returns>The mention string.</returns>
        string GetRoleMention(string roleName);
    }
}