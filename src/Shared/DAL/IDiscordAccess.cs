namespace HoU.GuildBot.Shared.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Enums;

    public interface IDiscordAccess
    {
        /// <summary>
        /// Tries to establish a connection to Discord.
        /// </summary>
        /// <param name="connectedHandler"><see cref="Func{TResult}"/> that will be invoked when the connection has been established.</param>
        /// <exception cref="ArgumentNullException"><paramref name="connectedHandler"/> is <b>null</b>.</exception>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task Connect(Func<Task> connectedHandler);

        /// <summary>
        /// Sets the <paramref name="gameName"/> as current bot game name.
        /// </summary>
        /// <param name="gameName">The game name.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SetCurrentGame(string gameName);

        /// <summary>
        /// Assigns the <paramref name="role"/> to the <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The ID of the user to assign the <paramref name="role"/> to.</param>
        /// <param name="role">The role to assign.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task AssignRole(ulong userId, Role role);

        /// <summary>
        /// Revokes the <paramref name="role"/> from the <paramref name="userId"/>.
        /// </summary>
        /// <param name="userId">The ID of the user to revoke the <paramref name="role"/> from.</param>
        /// <param name="role">The role to revoke.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task RevokeRole(ulong userId, Role role);

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
        bool IsUserOnline(ulong userID);

        /// <summary>
        /// Gets the user names for the given <paramref name="userIDs"/>.
        /// </summary>
        /// <param name="userIDs">The user IDs to get the names for.</param>
        /// <returns>A mapping from a userID to a display name.</returns>
        Dictionary<ulong, string> GetUserNames(IEnumerable<ulong> userIDs);

        /// <summary>
        /// Gets an array of class names available in a <paramref name="game"/>.
        /// </summary>
        /// <param name="game">The game to get the class names for.</param>
        /// <exception cref="ArgumentException"><paramref name="game"/> equals <see cref="Game.Undefined"/>.</exception>
        /// <returns>The classed.</returns>
        string[] GetClassNamesForGame(Game game);

        /// <summary>
        /// Revokes all current roles related with the <paramref name="userID"/> for the given <paramref name="game"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="game">The game.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task RevokeCurrentGameRoles(ulong userID, Game game);

        /// <summary>
        /// Sets the current role related with the <paramref name="userID"/> for the given <paramref name="game"/> to a specific <paramref name="className"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="game">The game.</param>
        /// <param name="className">The <paramref name="game"/> related class name.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SetCurrentGameRole(ulong userID, Game game, string className);
    }
}