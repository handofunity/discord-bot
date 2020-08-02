namespace HoU.GuildBot.Shared.DAL
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Enums;
    using JetBrains.Annotations;
    using Objects;
    using StrongTypes;

    public interface IDiscordAccess
    {
        /// <summary>
        /// Gets if the Discord access is connected to Discord.
        /// </summary>
        bool IsConnected { get; }

        /// <summary>
        /// Gets if the 'Hand of Unity' guild is available.
        /// </summary>
        bool IsGuildAvailable { get; }

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
        /// Sets the non-member related <paramref name="targetRole"/> for the given <paramref name="userID"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="targetRole">The target role.</param>
        /// <returns>True, if the role was added, otherwise false.</returns>
        Task<bool> TryAddNonMemberRole(DiscordUserID userID,
                                       Role targetRole);

        /// <summary>
        /// Revokes the non-member related <paramref name="targetRole"/> from the given <paramref name="userID"/>.
        /// </summary>
        /// <param name="userID">The user ID.</param>
        /// <param name="targetRole">The target role.</param>
        /// <returns>True, if the role was revoked, otherwise false.</returns>
        Task<bool> TryRevokeNonMemberRole(DiscordUserID userID,
                                          Role targetRole);

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
        /// Creates the <paramref name="message"/> in the servers welcome channel.
        /// </summary>
        /// <param name="message">The message to create.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task CreateBotMessageInWelcomeChannel(string message);

        /// <summary>
        /// Adds the <paramref name="reactions"/> to the given <paramref name="messageID"/> in the <paramref name="channelID"/>.
        /// </summary>
        /// <param name="channelID">The ID of the channel where the <paramref name="messageID"/> can be found.</param>
        /// <param name="messageID">The ID of the message to add the reactions to.</param>
        /// <param name="reactions">The reactions to add.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task AddReactionsToMessage(DiscordChannelID channelID, ulong messageID, EmojiDefinition[] reactions);

        /// <summary>
        /// Counts the guild members having the <paramref name="roleIDs"/>.
        /// </summary>
        /// <param name="roleIDs">The IDs of the roles to count.</param>
        /// <returns>The amount of guild members having the <paramref name="roleIDs"/>.</returns>
        int CountGuildMembersWithRoles(ulong[] roleIDs);

        /// <summary>
        /// Counts the guild members having the <paramref name="roleIDs"/> and not having the <paramref name="roleIDsToExclude"/>.
        /// </summary>
        /// <param name="roleIDs">The IDs of the roles to count.</param>
        /// <param name="roleIDsToExclude">The role IDs that will skip a count of the guild member.</param>
        /// <returns>The amount of guild members having the <paramref name="roleIDs"/> and not the <paramref name="roleIDsToExclude"/>.</returns>
        int CountGuildMembersWithRoles(ulong[] roleIDs, ulong[] roleIDsToExclude);

        /// <summary>
        /// Counts the guild members having the <paramref name="roleNames"/>.
        /// </summary>
        /// <param name="roleNames">The names of the roles to count.</param>
        /// <returns>The amount of guild members having the <paramref name="roleNames"/>.</returns>
        int CountGuildMembersWithRoles(string[] roleNames);

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

        /// <summary>
        /// Gets the name of the channel for the <paramref name="discordChannelID"/>.
        /// </summary>
        /// <param name="discordChannelID">The ID of the discord channel to get the name for.</param>
        /// <returns>The name of the channel.</returns>
        string GetChannelLocationAndName(DiscordChannelID discordChannelID);

        /// <summary>
        /// Creates a temporary voice channel.
        /// </summary>
        /// <param name="voiceChannelCategoryId">The ID of the category where the voice channel should be placed.</param>
        /// <param name="name">The name of the voice channel.</param>
        /// <param name="maxUsers">The maximum number of users allowed into the voice channel.</param>
        /// <returns>The ID of the created channel, or an error while creating it.</returns>
        Task<(ulong VoiceChannelId, string Error)> CreateVoiceChannel(ulong voiceChannelCategoryId,
                                                                      string name,
                                                                      int maxUsers);

        /// <summary>
        /// Deletes the voice channel, if it's empty.
        /// </summary>
        /// <param name="voiceChannelId">The ID of the voice channel to delete.</param>
        /// <returns><b>True</b>, if the voice channel was deleted or doesn't exist, otherwise <b>false</b>.</returns>
        Task<bool> DeleteVoiceChannelIfEmpty(ulong voiceChannelId);

        /// <summary>
        /// Gets the <see cref="DiscordChannelID"/> of the users voice channel, if he's in any.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>The <see cref="DiscordChannelID"/> of the users current voice channel.</returns>
        DiscordChannelID? GetUsersVoiceChannelId(DiscordUserID userId);

        /// <summary>
        /// Sets the mute state of all users in the <paramref name="voiceChannelId"/> below the bot to the value of <paramref name="mute"/>.
        /// </summary>
        /// <param name="voiceChannelId">The ID of the voice channel.</param>
        /// <param name="mute"><b>True</b>, if the users should be muted, otherwise <b>false</b>.</param>
        /// <returns><b>True</b>, when the bot is allowed to change the mute-state in the <paramref name="voiceChannelId"/> and successfully did, otherwise <b>false</b>.</returns>
        Task<bool> SetUsersMuteStateInVoiceChannel(DiscordChannelID voiceChannelId,
                                                   bool mute);

        /// <summary>
        /// Gets the ID of the avatar for the <paramref name="userId"/>, if the user has any avatar set.
        /// </summary>
        /// <param name="userId">The <see cref="DiscordUserID"/> of the user to get the avatar ID for.</param>
        /// <returns>The avatar ID, or <b>null</b>.</returns>
        [CanBeNull] string GetAvatarId(DiscordUserID userId);

        /// <summary>
        /// Gets all users that have one or more of the <paramref name="allowedRoles"/>.
        /// </summary>
        /// <param name="allowedRoles">The roles that are allowed for users in the result set.</param>
        /// <returns>All users with their allowed roles and detailed information.</returns>
        UserModel[] GetUsersInRoles(string[] allowedRoles);

        /// <summary>
        /// Gets the leadership mention string.
        /// </summary>
        /// <returns>A mention for Leaders and Officers.</returns>
        string GetLeadershipMention();

        /// <summary>
        /// Sends the <paramref name="embedData"/> as a notification in the <see cref="AppSettings.UnitsNotificationsChannelId"/>.
        /// </summary>
        /// <param name="embedData">The <see cref="EmbedData"/> to send.</param>
        /// <returns>An awaitable <see cref="Task"/>.</returns>
        Task SendUnitsNotificationAsync(EmbedData embedData);
    }
}