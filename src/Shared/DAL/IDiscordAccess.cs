﻿namespace HoU.GuildBot.Shared.DAL
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
        /// <exception cref="ArgumentNullException"><paramref name="connectedHandler"/> or <paramref name="disconnectedHandler"/> are <b>null</b>.</exception>
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
        /// Sets the primary role of the <paramref name="game"/> for the given <paramref name="userID"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="game">The game.</param>
        /// <returns>True, if the role was added, otherwise false.</returns>
        Task<bool> TryAddPrimaryGameRole(DiscordUserID userID, AvailableGame game);

        /// <summary>
        /// Revokes the primary role of the <paramref name="game"/> from the given <paramref name="userID"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="game">The game.</param>
        /// <returns>True, if the role was added, otherwise false.</returns>
        Task<bool> TryRevokePrimaryGameRole(DiscordUserID userID, AvailableGame game);

        /// <summary>
        /// Sets the role combined from <paramref name="game"/> and <paramref name="className"/> from the given <paramref name="userID"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="game">The game.</param>
        /// <param name="className">The <paramref name="game"/> related class name.</param>
        /// <returns>True, if the role was added, otherwise false.</returns>
        Task<bool> TryAddGameRole(DiscordUserID userID, AvailableGame game, string className);

        /// <summary>
        /// Revokes the role combined from <paramref name="game"/> and <paramref name="className"/> from the given <paramref name="userID"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="game">The game.</param>
        /// <param name="className">The <paramref name="game"/> related class name.</param>
        /// <returns>True, if the role was revoked, otherwise false.</returns>
        Task<bool> TryRevokeGameRole(DiscordUserID userID, AvailableGame game, string className);

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

        /// <summary>
        /// Gets all messages posted by the bot to the given <paramref name="channelID"/>.
        /// </summary>
        /// <param name="channelID">The ID of the channel to search for bot messages in.</param>
        /// <returns>An array of found messages.</returns>
        Task<TextMessage[]> GetBotMessagesInChannel(DiscordChannelID channelID);

        /// <summary>
        /// Deletes all messages of the bot in the given <paramref name="channelID"/>.
        /// </summary>
        /// <param name="channelID">The ID of the channel to delete the messages in.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task DeleteBotMessagesInChannel(DiscordChannelID channelID);

        /// <summary>
        /// Deletes a specific <paramref name="messageID"/> of the bot in the given <paramref name="channelID"/>.
        /// </summary>
        /// <param name="channelID">The ID of the channel to delete the message in.</param>
        /// <param name="messageID">The ID of the message to delete.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task DeleteBotMessageInChannel(DiscordChannelID channelID, ulong messageID);

        /// <summary>
        /// Creates the <paramref name="messages"/> in the given <paramref name="channelID"/>.
        /// </summary>
        /// <param name="channelID">The ID of the channel to create the <paramref name="messages"/> in.</param>
        /// <param name="messages">The messages to create.</param>
        /// <returns>An array of the message IDs.</returns>
        Task<ulong[]> CreateBotMessagesInChannel(DiscordChannelID channelID, string[] messages);

        /// <summary>
        /// Adds the <paramref name="reactions"/> to the given <paramref name="messageID"/> in the <paramref name="channelID"/>.
        /// </summary>
        /// <param name="channelID">The ID of the channel where the <paramref name="messageID"/> can be found.</param>
        /// <param name="messageID">The ID of the message to add the reactions to.</param>
        /// <param name="reactions">The reactions to add.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task AddReactionsToMessage(DiscordChannelID channelID, ulong messageID, string[] reactions);

        /// <summary>
        /// Counts the members having the <paramref name="roleName"/>.
        /// </summary>
        /// <param name="roleName">The name of the role to count.</param>
        /// <returns>The amount of people having the <paramref name="roleName"/>.</returns>
        int CountMembersWithRole(string roleName);

        /// <summary>
        /// Checks if the given <paramref name="roleID"/> exists.
        /// </summary>
        /// <param name="roleID">The ID of the role to check.</param>
        /// <returns><b>True</b>, if the role exists, otherwise false.</returns>
        bool DoesRoleExist(ulong roleID);

        /// <summary>
        /// Gets the current display name for the <paramref name="userID"/>.
        /// </summary>
        /// <param name="userID">The ID of the user to get the current display name for.</param>
        /// <returns>The nickname, if it's set, otherwise the username.</returns>
        string GetCurrentDisplayName(DiscordUserID userID);
    }
}