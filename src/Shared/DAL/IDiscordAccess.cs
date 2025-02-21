﻿namespace HoU.GuildBot.Shared.DAL;

public interface IDiscordAccess : IDiscordLogger
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
    Task ConnectAsync(Func<Task> connectedHandler,
                      Func<Task> disconnectedHandler);

    /// <summary>
    /// Sets the <paramref name="gameName"/> as current bot game name.
    /// </summary>
    /// <param name="gameName">The game name.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SetCurrentGameAsync(string gameName);

    /// <summary>
    /// Checks if the user with the <paramref name="userId"/> is currently online.
    /// </summary>
    /// <param name="userId">The Id of the user to check.</param>
    /// <returns><b>True</b>, if the user is online, otherwise <b>false</b>.</returns>
    bool IsUserOnline(DiscordUserId userId);

    /// <summary>
    /// Gets the user name for the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id to get the name for.</param>
    /// <returns>The display name of the user if found, otherwise <b>null</b>.</returns>
    string? GetUserDisplayName(DiscordUserId userId);

    /// <summary>
    /// Gets the user names for the given <paramref name="userIds"/>.
    /// </summary>
    /// <param name="userIds">The user Ids to get the names for.</param>
    /// <returns>A mapping from a userId to a display name.</returns>
    Dictionary<DiscordUserId, string> GetUserDisplayNames(IEnumerable<DiscordUserId> userIds);

    /// <summary>
    /// Sets the non-member related <paramref name="targetRole"/> for the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="targetRole">The target role.</param>
    /// <returns>True, if the role was added, otherwise false.</returns>
    Task<bool> TryAddNonMemberRoleAsync(DiscordUserId userId,
                                        Role targetRole);

    /// <summary>
    /// Sets the non-member related <paramref name="targetRole"/> for the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="targetRole">The target role.</param>
    /// <returns>True, if the role was added, otherwise false. Includes the role name.</returns>
    Task<(bool Success, string RoleName)> TryAddNonMemberRoleAsync(DiscordUserId userId,
                                                                   DiscordRoleId targetRole);

    /// <summary>
    /// Revokes the non-member related <paramref name="targetRole"/> from the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="targetRole">The target role.</param>
    /// <returns>True, if the role was revoked, otherwise false.</returns>
    Task<bool> TryRevokeNonMemberRoleAsync(DiscordUserId userId,
                                           Role targetRole);

    /// <summary>
    /// Revokes the non-member related <paramref name="targetRole"/> from the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="targetRole">The target role.</param>
    /// <returns>True, if the role was revoked, otherwise false. Includes the role name.</returns>
    Task<(bool Success, string RoleName)> TryRevokeNonMemberRoleAsync(DiscordUserId userId,
                                                                      DiscordRoleId targetRole);

    /// <summary>
    /// Assigns the <paramref name="roleId"/> to the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="roleId">The Id of the role to assign to the user.</param>
    /// <returns>True, if the role was assigned, otherwise false.</returns>
    Task<bool> TryAssignRoleAsync(DiscordUserId userId,
                                  DiscordRoleId roleId);

    /// <summary>
    /// Revokes the <paramref name="roleId"/> from the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The user Id.</param>
    /// <param name="roleId">The Id of the role to revoke from the user.</param>
    /// <returns>True, if the role was revoked, otherwise false.</returns>
    Task<bool> TryRevokeGameRoleAsync(DiscordUserId userId,
                                      DiscordRoleId roleId);

    /// <summary>
    /// Checks if the bot can manage roles for a specific <paramref name="userId"/>, depending on the guilds role configuration.
    /// </summary>
    /// <param name="userId">The Id of the user to change roles for.</param>
    /// <returns><b>True</b>, if the bot can manage roles for the user, otherwise <b>false</b>.</returns>
    bool CanManageRolesForUser(DiscordUserId userId);

    /// <summary>
    /// Gets the mention string for the give <paramref name="roleName"/>.
    /// </summary>
    /// <param name="roleName">The name of the role to get the mention for.</param>
    /// <exception cref="InvalidOperationException">No role with the given <paramref name="roleName"/> exists.</exception>
    /// <returns>The mention string.</returns>
    string GetRoleMention(string roleName);

    /// <summary>
    /// Gets all messages posted by the bot to the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to search for bot messages in.</param>
    /// <returns>An array of found messages.</returns>
    Task<TextMessage[]> GetBotMessagesInChannelAsync(DiscordChannelId channelId);

    /// <summary>
    /// Deletes all messages of the bot in the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to delete the messages in.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteBotMessagesInChannelAsync(DiscordChannelId channelId);

    /// <summary>
    /// Deletes a specific <paramref name="messageId"/> of the bot in the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to delete the message in.</param>
    /// <param name="messageId">The Id of the message to delete.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteBotMessageInChannelAsync(DiscordChannelId channelId,
                                        ulong messageId);

    /// <summary>
    /// Creates the <paramref name="messages"/> in the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to create the <paramref name="messages"/> in.</param>
    /// <param name="messages">The messages to create.</param>
    /// <returns>An array of the message Ids.</returns>
    Task<ulong[]> CreateBotMessagesInChannelAsync(DiscordChannelId channelId,
                                                  string[] messages);

    /// <summary>
    /// Creates the <paramref name="messages"/> with their <see cref="SelectMenuComponent"/>s in the given <paramref name="channelId"/>.
    /// </summary>
    /// <param name="channelId">The Id of the channel to create the <paramref name="messages"/> in.</param>
    /// <param name="messages">The messages to create with their <see cref="SelectMenuComponent"/>s.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="messages"/> contain a message with:
    /// - more than 25 <see cref="ButtonComponent"/>s in total
    /// or
    /// - more than 5 <see cref="ButtonComponent"/>s per action row
    /// or
    /// - more than 5 <see cref="SelectMenuComponent"/>s
    /// or
    /// - more than 1 <see cref="SelectMenuComponent"/> per action row
    /// or
    /// - more than 25 <see cref="SelectMenuComponent.Options"/> per <see cref="SelectMenuComponent"/>.</exception>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task CreateBotMessagesInChannelAsync(DiscordChannelId channelId,
                                         (string Content, ActionComponent[] Components)[] messages);

    /// <summary>
    /// Creates the <paramref name="message"/> in the servers welcome channel.
    /// </summary>
    /// <param name="message">The message to create.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task CreateBotMessageInWelcomeChannelAsync(string message);

    /// <summary>
    /// Counts the guild members having the <paramref name="roleIds"/>.
    /// </summary>
    /// <param name="roleIds">The Ids of the roles to count.</param>
    /// <returns>The amount of guild members having the <paramref name="roleIds"/>.</returns>
    int CountGuildMembersWithRoles(DiscordRoleId[] roleIds);

    /// <summary>
    /// Counts the guild members having the <paramref name="roleIds"/> and not having the <paramref name="roleIdsToExclude"/>.
    /// </summary>
    /// <param name="roleIds">The Ids of the roles to count.</param>
    /// <param name="roleIdsToExclude">The role Ids that will skip a count of the guild member.</param>
    /// <returns>The amount of guild members having the <paramref name="roleIds"/> and not the <paramref name="roleIdsToExclude"/>.</returns>
    int CountGuildMembersWithRoles(DiscordRoleId[]? roleIds,
                                   DiscordRoleId[] roleIdsToExclude);

    /// <summary>
    /// Counts the guild members having the <paramref name="roleNames"/>.
    /// </summary>
    /// <param name="roleNames">The names of the roles to count.</param>
    /// <returns>The amount of guild members having the <paramref name="roleNames"/>.</returns>
    int CountGuildMembersWithRoles(string[] roleNames);

    /// <summary>
    /// Gets the current display name for the <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The Id of the user to get the current display name for.</param>
    /// <returns>The nickname, if it's set, otherwise the username.</returns>
    string GetCurrentDisplayName(DiscordUserId userId);

    /// <summary>
    /// Gets the name of the channel for the <paramref name="discordChannelId"/>.
    /// </summary>
    /// <param name="discordChannelId">The Id of the discord channel to get the name for.</param>
    /// <returns>The name of the channel, if found, otherwise <b>null</b>.</returns>
    string? GetChannelLocationAndName(DiscordChannelId discordChannelId);

    /// <summary>
    /// Creates a temporary category channel.
    /// </summary>
    /// <param name="afterDiscordCategoryChannelId">The Id of the category after which to put the temporary category.</param>
    /// <param name="name">The name of the category.</param>
    /// <returns>The Id of the created category channel, or an error while creating it.</returns>
    Task<(DiscordCategoryChannelId CategoryChannelId, string? Error)> CreateCategoryChannelAsync(DiscordCategoryChannelId afterCategoryChannelId,
        string name);

    /// <summary>
    /// Creates a temporary voice channel.
    /// </summary>
    /// <param name="voiceChannelsCategoryId">The Id of the category where the voice channel should be placed.</param>
    /// <param name="name">The name of the voice channel.</param>
    /// <param name="maxUsers">The maximum number of users allowed into the voice channel.</param>
    /// <returns>The Id of the created channel, or an error while creating it.</returns>
    Task<(DiscordChannelId VoiceChannelId, string? Error)> CreateVoiceChannelAsync(DiscordCategoryChannelId voiceChannelsCategoryId,
        string name,
        int maxUsers);

    /// <summary>
    /// Deletes the category channel.
    /// </summary>
    /// <param name="categoryChannelId">The Id of the category channel to delete.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteCategoryChannelAsync(DiscordCategoryChannelId categoryChannelId);

    /// <summary>
    /// Gets the Id of the avatar for the <paramref name="userId"/>, if the user has any avatar set.
    /// </summary>
    /// <param name="userId">The <see cref="DiscordUserId"/> of the user to get the avatar Id for.</param>
    /// <returns>The avatar Id, or <b>null</b>.</returns>
    string? GetAvatarId(DiscordUserId userId);

    /// <summary>
    /// Gets all users on the Discord server.
    /// </summary>
    /// <returns>All users with their roles and detailed information.</returns>
    Task<UserModel[]> GetAllUsersAsync();

    /// <summary>
    /// Gets the leadership mention string.
    /// </summary>
    /// <returns>A mention for Leaders and Officers.</returns>
    string GetLeadershipMention();

    Task ArchiveThreadAsync(DiscordChannelId threadId);

    /// <summary>
    /// Sends the <paramref name="embedData"/> as a notification in the UnitsNotificationsChannel and creates a thread for the notification.
    /// </summary>
    /// <param name="unitsEndpointId">The Id of the <see cref="UnitsEndpoint"/> used to determine the channel the notification will be sent to.</param>
    /// <param name="context">The <see cref="UnitsContext"/> to use for the notification.</param>
    /// <param name="threadName">The name of the thread to create.</param>
    /// <param name="embedData">The <see cref="EmbedData"/> to send.</param>
    /// <returns>The Id of the created thread, or <b>null</b>, if the creation fails.</returns>
    Task<DiscordChannelId?> SendUnitsNotificationAsync(int unitsEndpointId,
        UnitsContext context,
        string threadName,
        EmbedData embedData);

    /// <summary>
    /// Sends the <paramref name="embedData"/> as a notification in the given thread.
    /// </summary>
    /// <param name="threadId">The Id of the Discord thread channel the notification will be sent to.</param>
    /// <param name="message">The message to send.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SendUnitsNotificationAsync(DiscordChannelId threadId,
                                    string message);

    /// <summary>
    /// Sends the <paramref name="embedData"/> as a notification in the UnitsNotificationsChannel and creates a thread for the notification.
    /// </summary>
    /// <param name="unitsEndpointId">The Id of the <see cref="UnitsEndpoint"/> used to determine the channel the notification will be sent to.</param>
    /// <param name="context">The <see cref="UnitsContext"/> to use for the notification.</param>
    /// <param name="threadName">The name of the thread to create.</param>
    /// <param name="embedData">The <see cref="EmbedData"/> to send.</param>
    /// <param name="mentions">The mentions for users or roles to notify about the <paramref name="embedData"/>.</param>
    /// <param name="mentionInThread">If <b>true</b>, the <paramref name="mentions"/> will be posted in the created thread, otherwise on the root message.</param>
    /// <returns>The Id of the created thread, or <b>null</b>, if the creation fails.</returns>
    Task<DiscordChannelId?> SendUnitsNotificationAsync(int unitsEndpointId,
        UnitsContext context,
        string threadName,
        EmbedData embedData,
        string[] mentions,
        bool mentionInThread);

    /// <summary>
    /// Sends the <paramref name="embedData"/> as a notification in the UnitsNotificationsChannel.
    /// </summary>
    /// <param name="threadId">The Id of the Discord thread channel the notification will be sent to.</param>
    /// <param name="message">The message to send.</param>
    /// <param name="mentions">The mentions for users or roles to notify about the <paramref name="embedData"/>.</param>
    /// <param name="linkToChannelId">The optional Id of a channel to link to.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SendUnitsNotificationAsync(DiscordChannelId threadId,
        string message,
        string[] mentions,
        DiscordChannelId? linkToChannelId);

    /// <summary>
    /// Gets all users in all voice channels under the given <paramref name="categoryChannelId"/>.
    /// </summary>
    /// <param name="categoryChannelid">The Id of the category channel to check.</param>
    /// <returns>A list of distinct user ids in the voice channels of the category.</returns>
    List<DiscordUserId> GetUsersInVoiceChannels(DiscordCategoryChannelId categoryChannelId);

    /// <summary>
    /// Tries to find a channel by its <paramref name="childChannelName"/> under the <paramref name="parentCategoryChannelId"/>.
    /// </summary>
    /// <param name="parentCategoryChannelId">The Id of the category to search in.</param>
    /// <param name="childChannelName">The name of the channel to find.</param>
    /// <returns>The Id of the found channel, otherwise <b>null</b>.</returns>
    DiscordChannelId? TryFindChannelInCategory(DiscordCategoryChannelId parentCategoryChannelId,
        string childChannelName);

    /// <summary>
    /// Gets all the <see cref="DiscordUserId"/>s of people having the <paramref name="roleId"/>.
    /// </summary>
    /// <param name="roleId">The Id of the role to check.</param>
    /// <returns>The list of <see cref="DiscordUserId"/> in the role, or empty.</returns>
    List<DiscordUserId> GetUsersIdsInRole(DiscordRoleId roleId);

    /// <summary>
    /// Ensures that the <see cref="AvailableGame.DisplayName"/> properties of the <paramref name="games"/> are set.
    /// </summary>
    /// <param name="games">The games to set the display names for.</param>
    void EnsureDisplayNamesAreSet(IEnumerable<AvailableGame> games);

    /// <summary>
    /// Ensures that the <see cref="AvailableGameRole.DisplayName"/> properties of the <paramref name="gameRoles"/> are set.
    /// </summary>
    /// <param name="gameRoles">The game roles to set the display names for.</param>
    void EnsureDisplayNamesAreSet(IEnumerable<AvailableGameRole> gameRoles);

    /// <summary>
    /// Gets the current roles for the given <paramref name="userId"/>.
    /// </summary>
    /// <param name="userId">The <see cref="DiscordUserId"/> of the user to get the roles for.</param>
    /// <returns>An array of all current roles the user has.</returns>
    DiscordRoleId[] GetUserRoles(DiscordUserId userId);

    /// <summary>
    /// Removes all roles for the given <paramref name="discordUserId"/>.
    /// </summary>
    /// <param name="discordUserId">The Id of the user to remove all roles from.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task RevokeAllRolesAsync(DiscordUserId discordUserId);

    /// <summary>
    /// Tries to add the <paramref name="userIds"/> to the given thread.
    /// </summary>
    /// <param name="threadId">The Id of the thread to add the <paramref name="userIds"/> to.</param>
    /// <param name="userIds">The user Ids to add.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task TryAddUsersToThreadAsync(DiscordChannelId threadId, DiscordUserId[] userIds);

    Task DebugAsync();
}