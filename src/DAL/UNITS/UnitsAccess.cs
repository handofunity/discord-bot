using UnitsEndpoint = HoU.GuildBot.Shared.Objects.UnitsEndpoint;

namespace HoU.GuildBot.DAL.UNITS;

public class UnitsAccess(IBearerTokenManager _bearerTokenManager,
                         IDiscordSyncClient _discordSyncClient,
                         IDiscordUserClient _discordUserClient,
                         ILogger<UnitsAccess> _logger)
    : IUnitsAccess
{
    private void Use(IUnitsBaseClient client, UnitsEndpoint unitsEndpoint)
    {
        client.BaseUrl = unitsEndpoint.BaseAddress.ToString();
        client.AuthorizationEndpoint = unitsEndpoint.KeycloakEndpoint;
        client.BearerTokenManager = _bearerTokenManager;
    }

    async Task IUnitsAccess.SendCreatedThreadIdAsync(UnitsEndpoint unitsEndpoint, int appointmentId, DiscordChannelId threadId)
    {
        Use(_discordSyncClient, unitsEndpoint);

        try
        {
            await _discordSyncClient.PushCreatedThreadIdAsync(appointmentId, (ulong)threadId);
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e, "Failed to send created thread id {ThreadId} for {AppointmentId}",
                threadId,
                appointmentId);
        }
    }

    async Task IUnitsAccess.SendCreatedVoiceChannelsAsync(UnitsEndpoint unitsEndpoint,
                                                          SyncCreatedVoiceChannelsRequest createdVoiceChannelsRequest)
    {
        Use(_discordSyncClient, unitsEndpoint);

        try
        {
            await _discordSyncClient.PushCreatedVoiceChannelsAsync(createdVoiceChannelsRequest);
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e, "Failed to send created voice channels for {AppointmentId}", createdVoiceChannelsRequest.AppointmentId);
        }
    }

    async Task IUnitsAccess.SendCurrentAttendeesAsync(UnitsEndpoint unitsEndpoint,
                                                      SyncCurrentAttendeesRequest currentAttendeesRequest)
    {
        Use(_discordSyncClient, unitsEndpoint);
        try
        {
            await _discordSyncClient.PushCurrentAttendeesAsync(currentAttendeesRequest);
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e, "Failed to send current attendees for {AppointmentId}", currentAttendeesRequest.AppointmentId);
        }
    }

    async Task<ProfileInfoResponse?> IUnitsAccess.GetProfileDataAsync(UnitsEndpoint unitsEndpoint,
                                                                      DiscordUserId discordUserId)
    {
        Use(_discordUserClient, unitsEndpoint);
        try
        {
            var response = await _discordUserClient.GetUserProfileAsync((ulong)discordUserId);
            return response.Result;
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e,
                             "Failed to get user profile of {DiscordUserId} from UNITS at {UnitsEndpoint}",
                             discordUserId,
                             unitsEndpoint.BaseAddress);
            return null;
        }
    }

    async Task<DiscordLeaderboardResponse?> IUnitsAccess.GetHeritageLeaderboardAsync(UnitsEndpoint unitsEndpoint)
    {
        Use(_discordUserClient, unitsEndpoint);
        try
        {
            var response = await _discordUserClient.GetHeritageLeaderboardAsync();
            return response.Result;
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e,
                             "Failed to get heritage leaderboard from UNITS at {UnitsEndpoint}",
                             unitsEndpoint.BaseAddress);
            return null;
        }
    }

    async Task<DiscordLeaderboardResponse?> IUnitsAccess.GetCurrentSeasonLeaderboardAsync(UnitsEndpoint unitsEndpoint)
    {
        Use(_discordUserClient, unitsEndpoint);
        try
        {
            var response = await _discordUserClient.GetCurrentSeasonLeaderboardAsync();
            return response.Result;
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e,
                             "Failed to get current season leaderboard from UNITS at {UnitsEndpoint}",
                             unitsEndpoint.BaseAddress);
            return null;
        }
    }
}