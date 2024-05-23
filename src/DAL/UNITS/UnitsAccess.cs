using UnitsEndpoint = HoU.GuildBot.Shared.Objects.UnitsEndpoint;

namespace HoU.GuildBot.DAL.UNITS;

public class UnitsAccess : IUnitsAccess
{
    private readonly IBearerTokenManager _bearerTokenManager;
    private readonly IDiscordSyncClient _discordSyncClient;
    private readonly ILogger<UnitsAccess> _logger;

    public UnitsAccess(IBearerTokenManager bearerTokenManager,
                       IDiscordSyncClient discordSyncClient,
                       ILogger<UnitsAccess> logger)
    {
        _bearerTokenManager = bearerTokenManager ?? throw new ArgumentNullException(nameof(bearerTokenManager));
        _discordSyncClient = discordSyncClient;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private void Use(UnitsEndpoint unitsEndpoint)
    {
        _discordSyncClient.BaseUrl = unitsEndpoint.BaseAddress.ToString();
        _discordSyncClient.AuthorizationEndpoint = unitsEndpoint.KeycloakEndpoint;
        _discordSyncClient.BearerTokenManager = _bearerTokenManager;
    }

    async Task IUnitsAccess.SendCreatedVoiceChannelsAsync(UnitsEndpoint unitsEndpoint,
                                                          SyncCreatedVoiceChannelsRequest createdVoiceChannelsRequest)
    {
        Use(unitsEndpoint);

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
        Use(unitsEndpoint);
        try
        {
            var response = await _discordSyncClient.GetUserProfileAsync((ulong)discordUserId);
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
}