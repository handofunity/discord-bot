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
            await _discordSyncClient.PushCreatedThreadIdAsync(appointmentId,
                (ulong)threadId);
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e, "Failed to send created thread id {ThreadId} for {AppointmentId}",
                threadId,
                appointmentId);
        }
    }

    async Task IUnitsAccess.SendCreatedThreadIdForRequisitionOrderAsync(UnitsEndpoint unitsEndpoint,
        int requisitionOrderId,
        DiscordChannelId threadId)
    {
        Use(_discordSyncClient, unitsEndpoint);

        try
        {
            await _discordSyncClient.PushCreatedThreadIdForRequisitionOrderAsync(requisitionOrderId,
                (ulong)threadId);
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e, "Failed to send created thread id {ThreadId} for {RequisitionOrderId}",
                threadId,
                requisitionOrderId);
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

    async Task<Dictionary<DiscordUserId, long>?> IUnitsAccess.GetHeritageTokensAsync(UnitsEndpoint unitsEndpoint)
    {
        Use(_discordSyncClient, unitsEndpoint);
        try
        {
            var response = await _discordSyncClient.GetHeritageTokensAsync();
            if (response.Result is null)
                return null;

            return response.Result
                .Select(m =>
                {
                    var isValid = ulong.TryParse(m.DiscordUserId, out var discordUserId);
                    return new
                    {
                        DiscordUserId = isValid ? discordUserId : 0,
                        m.HeritageTokens
                    };
                })
                .Where(m => m.DiscordUserId != 0)
                .ToDictionary(m => (DiscordUserId)m.DiscordUserId,
                    m => m.HeritageTokens);
        }
        catch (SwaggerException e)
        {
            _logger.LogError(e,
                "Failed to get heritage tokens from UNITS at {UnitsEndpoint}",
                unitsEndpoint.BaseAddress);
            return null;
        }
    }
}