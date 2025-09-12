namespace HoU.GuildBot.Shared.DAL;

public interface IDatabaseAccess
{
    Task<User[]> GetAllUsers();

    /// <summary>
    /// Adds a given set of user Ids to the database.
    /// </summary>
    /// <param name="userIds">The Ids to add.</param> 
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    /// <remarks>User Ids that are already present on the database won't be added.</remarks>
    Task EnsureUsersExistAsync(IEnumerable<DiscordUserId> userIds);

    /// <summary>
    /// Adds a user Id to the database.
    /// </summary>
    /// <param name="userId">The Id to add.</param>
    /// <returns>The <see cref="User"/> object that was either queried from the database or added.</returns>
    Task<(User User, bool IsNew)> GetOrAddUserAsync(DiscordUserId userId);

    /// <summary>
    /// Gets the current content for a message with a specific <paramref name="name"/>.
    /// </summary>
    /// <param name="name">The name (key) of the message.</param>
    /// <returns>The message content, if found, otherwise <b>null</b>.</returns>
    Task<string?> GetMessageContentAsync(string name);
    
    /// <summary>
    /// Adds a vacation duration for a specific <paramref name="user"/>, if the vacation is not colliding with another vacation of the same user.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="start">The start date of the vacation.</param>
    /// <param name="end">The end date of the vacation.</param>
    /// <param name="note">An optional note for the vacation.</param>
    /// <returns><b>True</b>, if the vacation could be added without any collisions, otherwise <b>false</b>.</returns>
    Task<bool> AddVacationAsync(User user, DateTime start, DateTime end, string? note);

    /// <summary>
    /// Deletes a matching vacation for the <paramref name="user"/>, <paramref name="start"/> and <paramref name="end"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="start">The start date of the vacation.</param>
    /// <param name="end">The end date of the vacation.</param>
    /// <returns><b>True</b>, if a matching vacation was found and deleted, otherwise <b>false</b>.</returns>
    Task<bool> DeleteVacationAsync(User user, DateTime start, DateTime end);
        
    /// <summary>
    /// Deletes all vacations that are in the past.
    /// </summary>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeletePastVacationsAsync();

    /// <summary>
    /// Deletes all vacations having a relation with the <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteVacationsAsync(User user);

    /// <summary>
    /// Gets all upcoming vacations.
    /// </summary>
    /// <returns>An array of tupples, containing the user Id, the vacation start and end, as well as an optional note.</returns>
    Task<(DiscordUserId UserId, DateTime Start, DateTime End, string? Note)[]> GetVacationsAsync();

    /// <summary>
    /// Gets all vacations for a specific <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to get the vacations for.</param>
    /// <returns>An array of tupples, containing the user Id, the vacation start and end, as well as an optional note.</returns>
    Task<(DiscordUserId UserId, DateTime Start, DateTime End, string? Note)[]> GetVacationsAsync(User user);

    /// <summary>
    /// Gets all vacations for a specific <paramref name="date"/>.
    /// </summary>
    /// <param name="date">The date to get the vacations for.</param>
    /// <returns>An array of tupples, containing the user Id, the vacation start and end, as well as an optional note.</returns>
    Task<(DiscordUserId UserId, DateTime Start, DateTime End, string? Note)[]> GetVacationsAsync(DateTime date);

    /// <summary>
    /// Gets all available games.
    /// </summary>
    /// <returns>An array of available games.</returns>
    Task<AvailableGame[]> GetAvailableGamesAsync();

    /// <summary>
    /// Updates the <paramref name="lastSeen"/> info for the given <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to update the last seen info for.</param>
    /// <param name="lastSeen">The timestamp to use as info.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task UpdateUserInfoLastSeenAsync(User user, DateTime lastSeen);
    
    /// <summary>
    /// Updates the promotion to Trial Member date info for the given <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user to update the date for.</param>
    /// <remarks>Set the <see cref="User.PromotedToTrialMemberDate"/> to <b>null</b> when promoted to regular member or trial member gets revoked.</remarks>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task UpdateUserInfoPromotionToTrialMemberDateAsync(User user);

    /// <summary>
    /// Updates the user information for the given <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users to update the information for.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task UpdateUserInformationAsync(IEnumerable<User> users);

    /// <summary>
    /// Gets the last seen info for the given <paramref name="users"/>.
    /// </summary>
    /// <param name="users">The users to fetch the last seen info for.</param>
    /// <exception cref="ArgumentNullException"><paramref name="users"/> is <b>null</b>.</exception>
    /// <exception cref="ArgumentException"><paramref name="users"/> is empty.</exception>
    /// <returns>An array of tipples, containing the user Id and the last seen timestamp. The timestamp is <b>null</b>, if no info is available.</returns>
    Task<(InternalUserId UserId, DateTime? LastSeen)[]> GetLastSeenInfoForUsersAsync(User[] users);

    /// <summary>
    /// Deletes the user info related to the <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task DeleteUserInfoAsync(User user);

    /// <summary>
    /// Sets the user birthday related to the <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <param name="birthday">The date of the birthday.</param>
    /// <returns><b>True</b>, if the birthday was set, otherwise <b>false</b>.</returns>
    Task<bool> SetBirthdayAsync(User user,
                                DateOnly birthday);

    /// <summary>
    /// Deletes the user birthday related to the <paramref name="user"/>.
    /// </summary>
    /// <param name="user">The user.</param>
    /// <returns><b>True</b>, if the birthday was deleted, otherwise <b>false</b>.</returns>
    Task<bool> DeleteUserBirthdayAsync(User user);

    /// <summary>
    /// Tries to get the internal Id of the game associated with the <paramref name="primaryGameDiscordRoleId"/>.
    /// </summary>
    /// <param name="primaryGameDiscordRoleId">The Discord Id of the primary game role.</param>
    /// <returns>The internal Id of the game, if it was found, otherwise <b>null</b>.</returns>
    Task<InternalGameId?> TryGetInternalGameIdAsync(DiscordRoleId primaryGameDiscordRoleId);

    /// <summary>
    /// Tries to get the internal Id of the game role associated with the <paramref name="discordRoleId"/>.
    /// </summary>
    /// <param name="discordRoleId">The Discord Id of the role.</param>
    /// <returns>The internal Id of the game role, if it was found, otherwise <b>null</b>.</returns>
    Task<InternalGameRoleId?> TryGetInternalGameRoleIdAsync(DiscordRoleId discordRoleId);

    /// <summary>
    /// Tries to add the game.
    /// </summary>
    /// <param name="userId">The <see cref="InternalUserId"/> of the user who adds the game.</param>
    /// <param name="primaryGameDiscordRoleId">The Discord Id of the primary game role.</param>
    /// <returns>A value tuple containing the success state, and, if the add fails, the error that occurred.</returns>
    Task<(bool Success, string? Error)> TryAddGameAsync(InternalUserId userId,
                                                        DiscordRoleId primaryGameDiscordRoleId);

    /// <summary>
    /// Tries to edit the game.
    /// </summary>
    /// <param name="userId">The Id of the user who edits the game.</param>
    /// <param name="gameId">The Id of the game to edit.</param>
    /// <param name="updated">The updated class holding the new values.</param>
    /// <returns>A value tuple containing the success state, and, if the add fails, the error that occurred.</returns>
    Task<(bool Success, string? Error)> TryUpdateGameAsync(InternalUserId userId,
                                                         InternalGameId gameId,
                                                         AvailableGame updated);

    /// <summary>
    /// Tries to remove a game and all dependencies.
    /// </summary>
    /// <param name="gameId">The internal Id of the game to remove.</param>
    /// <returns>A value tuple containing the success state, and, if the add fails, the error that occurred.</returns>
    Task<(bool Success, string? Error)> TryRemoveGameAsync(InternalGameId gameId);

    /// <summary>
    /// Tries to add the game role.
    /// </summary>
    /// <param name="userId">The Id of the user who adds the role.</param>
    /// <param name="internalGameId">The Id of the game to add the game role to.</param>
    /// <param name="discordRoleId">The DiscordId of the role.</param>
    /// <returns>A value tuple containing the success state, and, if the add fails, the error that occurred.</returns>
    Task<(bool Success, string? Error)> TryAddGameRoleAsync(InternalUserId userId,
                                                            InternalGameId internalGameId,
                                                            DiscordRoleId discordRoleId);
    
    /// <summary>
    /// Tries to remove the game role.
    /// </summary>
    /// <param name="gameRoleId">The internal Id of the game role to remove.</param>
    /// <returns>A value tuple containing the success state, and, if the add fails, the error that occurred.</returns>
    Task<(bool Success, string? Error)> TryRemoveGameRoleAsync(InternalGameRoleId gameRoleId);

    /// <summary>
    /// Gets the <see cref="InternalUserId"/> with a birthday on the given <paramref name="month"/> and <paramref name="day"/>.
    /// </summary>
    /// <param name="month">The month to query for.</param>
    /// <param name="day">The day to query for.</param>
    /// <returns>An array with the <see cref="InternalUserId"/> of all matching users.</returns>
    Task<InternalUserId[]> GetUsersWithBirthdayAsync(short month,
                                                     short day);

    /// <summary>
    /// Gets the last heritage tokens for all users.
    /// </summary>
    /// <returns>The dictionary containing the user Ids and their heritage tokens.</returns>
    Task<Dictionary<DiscordUserId, long>> GetLastHeritageTokensAsync();

    /// <summary>
    /// Persists the heritage tokens for all users.
    /// </summary>
    /// <param name="heritageTokens">The dictionary containing the user Ids and their heritage tokens.</param>
    Task PersistHeritageTokensAsync(Dictionary<DiscordUserId, long> heritageTokens);
}