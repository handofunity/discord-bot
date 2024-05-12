namespace HoU.GuildBot.Keycloak.BLL;

internal class KeycloakSyncService : IKeycloakSyncService
{
    private static readonly SemaphoreSlim SemaphoreSlim = new(1, 1);

    private readonly IDiscordAccess _discordAccess;
    private readonly IKeycloakDiscordComparer _keycloakDiscordComparer;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly IKeycloakUserGroupAggregator _keycloakUserGroupAggregator;
    private readonly IKeycloakUserGroupAssigner _keycloakUserGroupAssigner;
    private readonly IKeycloakUserCreator _keycloakUserCreator;
    private readonly IKeycloakUserCleaner _keycloakUserCleaner;
    private readonly IKeycloakUserUpdater _keycloakUserUpdater;
    private readonly ILogger<KeycloakSyncService> _logger;

    public KeycloakSyncService(IDiscordAccess discordAccess,
                               IKeycloakDiscordComparer keycloakDiscordComparer,
                               IDynamicConfiguration dynamicConfiguration,
                               IKeycloakUserGroupAggregator keycloakUserGroupAggregator,
                               IKeycloakUserGroupAssigner keycloakUserGroupAssigner,
                               IKeycloakUserCreator keycloakUserCreator,
                               IKeycloakUserCleaner keycloakUserCleaner,
                               IKeycloakUserUpdater keycloakUserUpdater,
                               ILogger<KeycloakSyncService> logger)
    {
        _discordAccess = discordAccess;
        _keycloakDiscordComparer = keycloakDiscordComparer;
        _dynamicConfiguration = dynamicConfiguration;
        _keycloakUserGroupAggregator = keycloakUserGroupAggregator;
        _keycloakUserGroupAssigner = keycloakUserGroupAssigner;
        _keycloakUserCreator = keycloakUserCreator;
        _keycloakUserCleaner = keycloakUserCleaner;
        _keycloakUserUpdater = keycloakUserUpdater;
        _logger = logger;
    }

    private bool IsValidEndpoint(KeycloakEndpoint endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint.BaseUrl.ToString()))
        {
            _logger.LogWarning("Keycloak base url is not configured");
            return false;
        }

        if (string.IsNullOrWhiteSpace(endpoint.AccessTokenUrl.ToString()))
        {
            _logger.LogWarning("Keycloak access token url is not configured");
            return false;
        }

        if (string.IsNullOrWhiteSpace(endpoint.ClientId))
        {
            _logger.LogWarning("Keycloak client id is not configured");
            return false;
        }

        if (string.IsNullOrWhiteSpace(endpoint.ClientSecret))
        {
            _logger.LogWarning("Keycloak client secret is not configured");
            return false;
        }

        if (string.IsNullOrWhiteSpace(endpoint.Realm))
        {
            _logger.LogWarning("Keycloak realm is not configured");
            return false;
        }

        return true;
    }

    private static void SanitizeDiscordUsersAndRoles(IEnumerable<UserModel> users,
                                                     ConfiguredKeycloakGroups configuredKeycloakGroups)
    {
        foreach (var userModel in users)
        {
            // Remove "a_" prefix of animated avatar IDs
            if (userModel.AvatarId?.StartsWith("a_") == true)
                userModel.AvatarId = userModel.AvatarId[2..];

            // Only keep roles that match to a Keycloak group.
            userModel.Roles = userModel.Roles
                                       .Join(configuredKeycloakGroups.DiscordRoleToKeycloakGroupMapping,
                                             id => id,
                                             pair => pair.Key,
                                             (id,
                                              _) => id)
                                       .ToArray();
        }
    }

    private async Task<KeycloakSyncResponse> SyncDiffToEndpointAsync(KeycloakEndpoint keycloakEndpoint,
                                                                     KeycloakDiscordDiff diff)
    {
        var addedUsers = await _keycloakUserCreator.CreateUsersAsync(keycloakEndpoint, diff);
        var (enabledUsers, disabledUsers, loggedOutUsers) = await _keycloakUserUpdater.UpdateUserActivationStateAsync(keycloakEndpoint, diff);
        var updatedDetails = await _keycloakUserUpdater.UpdateUserDetailsAsync(keycloakEndpoint, diff);
        var (assignedUserGroupRelationships, unassignedUserGroupRelationships) = await _keycloakUserGroupAssigner.UpdateUserGroupAssignments(keycloakEndpoint, diff);

        return new KeycloakSyncResponse(addedUsers,
                                        disabledUsers,
                                        loggedOutUsers,
                                        enabledUsers,
                                        updatedDetails,
                                        assignedUserGroupRelationships,
                                        unassignedUserGroupRelationships);
    }

    // Do NOT implement this as explicit implementation, as it cannot be triggered by hangfire then!
    public async Task SyncAllUsersAsync()
    {
        await SemaphoreSlim.WaitAsync();
        try
        {
            if (_dynamicConfiguration.KeycloakEndpoints.Length == 0)
            {
                _logger.LogWarning("No Keycloak access configured");
                return;
            }

            if (!_discordAccess.IsConnected || !_discordAccess.IsGuildAvailable)
                return;

            foreach (var keycloakEndpoint in _dynamicConfiguration.KeycloakEndpoints)
            {
                if (!IsValidEndpoint(keycloakEndpoint))
                    continue;

                var keycloakState = await _keycloakUserGroupAggregator.AggregateCurrentStateAsync(keycloakEndpoint);
                if (keycloakState is null)
                    continue;

                // Get Discord users and roles
                var allDiscordUsers = await _discordAccess.GetAllUsersAsync();
                SanitizeDiscordUsersAndRoles(allDiscordUsers, keycloakState.ConfiguredKeycloakGroups);

                // Get diff between Discord and Keycloak
                var diff = _keycloakDiscordComparer.GetDiff(keycloakState, allDiscordUsers);

                _logger.LogInformation("Sending {@Diff} to Keycloak at {Address} ...",
                                       diff,
                                       keycloakEndpoint.BaseUrl);
                var result = await SyncDiffToEndpointAsync(keycloakEndpoint, diff);

                _logger.LogInformation("Sent {@Result} to Keycloak at {Address}",
                                       result,
                                       keycloakEndpoint.BaseUrl);
                var utcNow = DateTime.UtcNow;
                var notificationRequired = utcNow is { Hour: 15, Minute: < 15 };
                var hasChanges = result.AddedUsers > 0
                              || result.DisabledUsers > 0
                              || result.LoggedOutUsers > 0
                              || result.EnabledUsers > 0
                              || result.UpdatedDetails > 0
                              || result.AssignedGroupMemberships > 0
                              || result.RemovedGroupMemberships > 0;
                var shouldPostResults = notificationRequired || hasChanges;
                if (!shouldPostResults)
                    continue;

                var sb = new StringBuilder($"**Keycloak Sync:** Synchronized {allDiscordUsers.Length} users with Keycloak");
                sb.AppendLine();
                if (result.AddedUsers > 0)
                    sb.AppendLine($"Added {result.AddedUsers} users.");
                if (result.DisabledUsers > 0)
                    sb.AppendLine($"Disabled {result.DisabledUsers} users.");
                if (result.LoggedOutUsers > 0)
                    sb.AppendLine($"Logged out {result.LoggedOutUsers} users.");
                if (result.EnabledUsers > 0)
                    sb.AppendLine($"Enabled {result.EnabledUsers} users.");
                if (result.UpdatedDetails > 0)
                    sb.AppendLine($"Updated {result.UpdatedDetails} user details (avatar, nickname, ...).");
                if (result.AssignedGroupMemberships > 0)
                    sb.AppendLine($"Assigned {result.AssignedGroupMemberships} users to groups.");
                if (result.RemovedGroupMemberships > 0)
                    sb.AppendLine($"Removed {result.RemovedGroupMemberships} users from groups.");

                await _discordAccess.LogToDiscordAsync(sb.ToString());
            }
        }
        finally
        {
            SemaphoreSlim.Release();
        }
    }

    // Do NOT implement this as explicit implementation, as it cannot be triggered by hangfire then!
    public async Task DeleteFlaggedUsersAsync()
    {
        if (_dynamicConfiguration.KeycloakEndpoints.Length == 0)
        {
            _logger.LogWarning("No Keycloak access configured");
            return;
        }

        foreach (var keycloakEndpoint in _dynamicConfiguration.KeycloakEndpoints)
        {
            if (!IsValidEndpoint(keycloakEndpoint))
                continue;

            await _keycloakUserCleaner.DeleteFlaggedUsersAsync(keycloakEndpoint);
        }
    }
}