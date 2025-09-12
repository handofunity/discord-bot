namespace HoU.GuildBot.Shared.DAL;

public interface IUnitsAccess
{
    /// <summary>
    /// Sends the <paramref name="threadId"/> for the <paramref name="appointmentId"/> to the UNIT system.
    /// </summary>
    /// <param name="unitsEndpoint">The data used to sync with the UNIT system.</param>
    /// <param name="appointmentId">The Id of the appointment the thread was created for.</param>
    /// <param name="threadId">The Id of the created thread.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SendCreatedThreadIdAsync(UnitsEndpoint unitsEndpoint,
        int appointmentId,
        DiscordChannelId threadId);

    /// <summary>
    /// Sends the <paramref name="threadId"/> for the <paramref name="requisitionOrderId"/> to the UNIT system.
    /// </summary>
    /// <param name="unitsEndpoint">The data used to sync with the UNIT system.</param>
    /// <param name="requisitionOrderId">The Id of the requisition order the thread was created for.</param>
    /// <param name="threadId">The Id of the created thread.</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SendCreatedThreadIdForRequisitionOrderAsync(UnitsEndpoint unitsEndpoint,
        int requisitionOrderId,
        DiscordChannelId threadId);

    /// <summary>
    /// Sends the <paramref name="createdVoiceChannelsRequest"/> to the UNIT system.
    /// </summary>
    /// <param name="unitsEndpoint">The data used to sync with the UNIT system.</param>
    /// <param name="createdVoiceChannelsRequest">The createdVoiceChannelsRequest containing the information about the created voice channels</param>
    /// <returns>An awaitable <see cref="Task"/>.</returns>
    Task SendCreatedVoiceChannelsAsync(UnitsEndpoint unitsEndpoint,
                                       SyncCreatedVoiceChannelsRequest createdVoiceChannelsRequest);

    /// <summary>
    /// Sends the <paramref name="currentAttendeesRequest"/> to the UNIT system.
    /// </summary>
    /// <param name="unitsEndpoint"></param>
    /// <param name="currentAttendeesRequest"></param>
    /// <returns></returns>
    Task SendCurrentAttendeesAsync(UnitsEndpoint unitsEndpoint,
                                   SyncCurrentAttendeesRequest currentAttendeesRequest);

    /// <summary>
    /// Retrieves the profile data asynchronously for a given <see cref="DiscordUserId"/>.
    /// </summary>
    /// <param name="unitsEndpoint">The <see cref="UnitsEndpoint"/> to query.</param>
    /// <param name="discordUserId">The <see cref="DiscordUserId"/> to retrieve the data for.</param>
    /// <returns>The profile data of the Discord user, if found, otherwise <b>null</b>..</returns>
    Task<ProfileInfoResponse?> GetProfileDataAsync(UnitsEndpoint unitsEndpoint,
                                                   DiscordUserId discordUserId);

    /// <summary>
    /// Retrieves the leaderboard for the heritage ranking.
    /// </summary>
    /// <param name="unitsEndpoint">The <see cref="UnitsEndpoint"/> to query.</param>
    /// <returns>The leaderboard data.</returns>
    Task<DiscordLeaderboardResponse?> GetHeritageLeaderboardAsync(UnitsEndpoint unitsEndpoint);

    /// <summary>
    /// Retrieves the leaderboard for the current season ranking.
    /// </summary>
    /// <param name="unitsEndpoint">The <see cref="UnitsEndpoint"/> to query.</param>
    /// <returns>The leaderboard data.</returns>
    Task<DiscordLeaderboardResponse?> GetCurrentSeasonLeaderboardAsync(UnitsEndpoint unitsEndpoint);

    /// <summary>
    /// Retrieves the heritage tokens for all users.
    /// </summary>
    /// <param name="unitsEndpoint">The <see cref="UnitsEndpoint"/> to query.</param>
    /// <returns>A dictionary mapping Discord user IDs to their heritage token counts.</returns>
    Task<Dictionary<DiscordUserId, long>?> GetHeritageTokensAsync(UnitsEndpoint unitsEndpoint);
}