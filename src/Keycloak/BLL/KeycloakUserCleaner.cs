namespace HoU.GuildBot.Keycloak.BLL;

internal class KeycloakUserCleaner : IKeycloakUserCleaner
{
    private readonly IKeycloakUserReader _keycloakUserReader;
    private readonly IKeycloakUserEraser _keycloakUserEraser;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IDiscordLogger _discordLogger;
    private readonly ILogger<KeycloakUserCleaner> _logger;

    public KeycloakUserCleaner(IKeycloakUserReader keycloakUserReader,
                               IKeycloakUserEraser keycloakUserEraser,
                               IDynamicConfiguration dynamicConfiguration,
                               IDiscordLogger discordLogger,
                               ILogger<KeycloakUserCleaner> logger)
    {
        _keycloakUserReader = keycloakUserReader;
        _keycloakUserEraser = keycloakUserEraser;
        _dynamicConfiguration = dynamicConfiguration;
        _discordLogger = discordLogger;
        _logger = logger;
    }

    private string GetDeveloperRoleMention()
    {
        var roleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping["DeveloperRoleId"];
        return roleId.ToMention();
    }

    private Task<KeycloakUserId[]?> GetUsersFlaggedForDeletionAsync(KeycloakEndpoint keycloakEndpoint) =>
        _keycloakUserReader.GetUsersFlaggedForDeletionAsync(keycloakEndpoint, DateOnly.FromDateTime(DateTime.UtcNow));

    private async Task FailedToFetchFlaggedUsersAsync(KeycloakEndpoint keycloakEndpoint)
    {
        _logger.LogWarning("Couldn't fetch users flagged for deletion from Keycloak instance at {Endpoint}", keycloakEndpoint.BaseUrl);
        await
            _discordLogger
               .LogToDiscordAsync($"**Keycloak Sync:** Couldn't fetch users flagged for deletion {GetDeveloperRoleMention()}");
    }

    private async Task SuccessfullyDeletedAllFlaggedUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                               int deletedCount)
    {
        _logger.LogInformation("Successfully deleted {Count} users from Keycloak instance at {Endpoint}",
                               deletedCount,
                               keycloakEndpoint.BaseUrl);
        await _discordLogger.LogToDiscordAsync($"**Keycloak Sync:** Successfully deleted {deletedCount} users");
    }

    private async Task PartiallyDeletedFlaggedUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                         int deletedCount,
                                                         int expectedCount)
    {
        _logger.LogWarning("Partially deleted {ActualCount} from {ExpectedCount} users from Keycloak instance at {Endpoint}",
                           deletedCount,
                           expectedCount,
                           keycloakEndpoint.BaseUrl);
        await _discordLogger.LogToDiscordAsync($"**Keycloak Sync:** Partially deleted {deletedCount} from {expectedCount} "
                                             + $"users {GetDeveloperRoleMention()}");
    }

    async Task IKeycloakUserCleaner.DeleteFlaggedUsersAsync(KeycloakEndpoint keycloakEndpoint)
    {
        var flaggedUsers = await GetUsersFlaggedForDeletionAsync(keycloakEndpoint);
        if (flaggedUsers is null)
        {
            await FailedToFetchFlaggedUsersAsync(keycloakEndpoint);
            return;
        }

        if (flaggedUsers.Length == 0)
            return;

        _logger.LogInformation("Starting to delete {Count} users", flaggedUsers.Length);

        try
        {
            var deletedCount = await _keycloakUserEraser.DeleteUsersAsync(keycloakEndpoint, flaggedUsers);
            if (deletedCount == flaggedUsers.Length)
                await SuccessfullyDeletedAllFlaggedUsersAsync(keycloakEndpoint, deletedCount);
            else
                await PartiallyDeletedFlaggedUsersAsync(keycloakEndpoint, deletedCount, flaggedUsers.Length);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to delete users in Keycloak instance at {Endpoint}", keycloakEndpoint.BaseUrl);
        }
    }
}