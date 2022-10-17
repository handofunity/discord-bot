namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class KeycloakSyncService : IKeycloakSyncService
{
    private readonly IDiscordAccess _discordAccess;
    private readonly IKeycloakAccess _keycloakAccess;
    private readonly IKeycloakDiscordComparer _keycloakDiscordComparer;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly ILogger<KeycloakSyncService> _logger;

    public KeycloakSyncService(IDiscordAccess discordAccess,
                               IKeycloakAccess keycloakAccess,
                               IKeycloakDiscordComparer keycloakDiscordComparer,
                               IDynamicConfiguration dynamicConfiguration,
                               ILogger<KeycloakSyncService> logger)
    {
        _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
        _keycloakAccess = keycloakAccess ?? throw new ArgumentNullException(nameof(keycloakAccess));
        _keycloakDiscordComparer = keycloakDiscordComparer ?? throw new ArgumentNullException(nameof(keycloakDiscordComparer));
        _dynamicConfiguration = dynamicConfiguration ?? throw new ArgumentNullException(nameof(dynamicConfiguration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private string GetDeveloperRoleMention()
    {
        var roleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping["DeveloperRoleId"];
        return roleId.ToMention();
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
            if (userModel.AvatarId != null && userModel.AvatarId.StartsWith("a_"))
                userModel.AvatarId = userModel.AvatarId.Substring(2);

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

    // Do NOT implement this as explicit implementation, as it cannot be triggered by hangfire then!
    public async Task SyncAllUsersAsync()
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

            // Get Keycloak groups (aka roles in Discord)
            var configuredKeycloakGroups = await _keycloakAccess.GetConfiguredGroupsAsync(keycloakEndpoint);
            if (configuredKeycloakGroups is null)
                continue;
            
            // Get Keycloak group members
            var keycloakGroupMembers = new Dictionary<KeycloakGroupId, KeycloakUserId[]>();
            foreach (var keycloakGroupId in configuredKeycloakGroups.DiscordRoleToKeycloakGroupMapping.Values)
            {
                var members = await _keycloakAccess.GetGroupMembersAsync(keycloakEndpoint, keycloakGroupId);
                if (members is not null)
                    keycloakGroupMembers.Add(keycloakGroupId, members);
            }

            var fallbackMembers = await _keycloakAccess.GetGroupMembersAsync(keycloakEndpoint,
                                                                             configuredKeycloakGroups.FallbackGroupId);
            if (fallbackMembers is not null)
                keycloakGroupMembers.Add(configuredKeycloakGroups.FallbackGroupId, fallbackMembers);

            // Get mapping of KeycloakUserId to DiscordUserId
            var userIdMapping = new Dictionary<KeycloakUserId, DiscordUserId>();
            var distinctKeycloakUserIds = keycloakGroupMembers.Values.SelectMany(m => m).Distinct();
            foreach (var keycloakUserId in distinctKeycloakUserIds)
            {
                var discordUserId = await _keycloakAccess.GetDiscordUserIdAsync(keycloakEndpoint, keycloakUserId);
                if (discordUserId is not null)
                    userIdMapping.Add(keycloakUserId, discordUserId.Value);
            }
            
            // Get currently disabled users
            var disabledUsers = await _keycloakAccess.GetUsersFlaggedForDeletionAsync(keycloakEndpoint, null);
            if (disabledUsers is null)
                continue;

            // Get Discord users and roles
            var allDiscordUsers = await _discordAccess.GetAllUsersAsync();
            SanitizeDiscordUsersAndRoles(allDiscordUsers, configuredKeycloakGroups);
            
            // Get diff between Discord and Keycloak
            var diff = _keycloakDiscordComparer.GetDiff(allDiscordUsers,
                                                        userIdMapping, 
                                                        configuredKeycloakGroups,
                                                        keycloakGroupMembers,
                                                        disabledUsers);
            
            _logger.LogInformation("Sending {@Diff} to Keycloak at {Address} ...",
                                   diff,
                                   keycloakEndpoint.BaseUrl);
            var result = await _keycloakAccess.SendDiffAsync(keycloakEndpoint, diff);
            if (result is null)
                continue;

            _logger.LogInformation("Sent {@Result} to Keycloak at {Address}",
                                   result,
                                   keycloakEndpoint.BaseUrl);
            var utcNow = DateTime.UtcNow;
            var notificationRequired = utcNow.Hour == 15 && utcNow.Minute < 15;
            var hasChanges = result.AddedUsers > 0
                          || result.DisabledUsers > 0
                          || result.LoggedOutUsers > 0
                          || result.EnabledUsers > 0
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
            if (result.AssignedGroupMemberships > 0)
                sb.AppendLine($"Assigned {result.AssignedGroupMemberships} users to groups.");
            if (result.RemovedGroupMemberships > 0)
                sb.AppendLine($"Removed {result.RemovedGroupMemberships} users from groups.");

            await _discordAccess.LogToDiscordAsync(sb.ToString());
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

            var flaggedUsers = await _keycloakAccess.GetUsersFlaggedForDeletionAsync(keycloakEndpoint,
                                                                                     DateOnly.FromDateTime(DateTime.UtcNow));
            if (flaggedUsers is null)
            {
                _logger.LogWarning("Couldn't fetch users flagged for deletion from Keycloak instance at {Endpoint}",
                                   keycloakEndpoint.BaseUrl);
                await
                    _discordAccess
                       .LogToDiscordAsync($"**Keycloak Sync:** Couldn't fetch users flagged for deletion {GetDeveloperRoleMention()}");
                continue;
            }

            if (flaggedUsers.Length == 0)
                return;

            _logger.LogInformation("Starting to delete {Count} users", flaggedUsers.Length);

            try
            {
                var deletedCount = await _keycloakAccess.DeleteUsersAsync(keycloakEndpoint, flaggedUsers);
                if (deletedCount == flaggedUsers.Length)
                {
                    _logger.LogInformation("Successfully deleted {Count} users from Keycloak instance at {Endpoint}",
                                           deletedCount,
                                           keycloakEndpoint.BaseUrl);
                    await
                        _discordAccess
                           .LogToDiscordAsync($"**Keycloak Sync:** Successfully deleted {deletedCount} users");
                }
                else
                {
                    _logger.LogWarning("Partially deleted {ActualCount} from {ExpectedCount} users from Keycloak instance at {Endpoint}",
                                       deletedCount,
                                       flaggedUsers.Length,
                                       keycloakEndpoint.BaseUrl);
                    await
                        _discordAccess
                           .LogToDiscordAsync($"**Keycloak Sync:** Partially deleted {deletedCount} from {flaggedUsers.Length} "
                                            + $"users {GetDeveloperRoleMention()}");
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to delete users in Keycloak instance at {Endpoint}",
                                 keycloakEndpoint.BaseUrl);
            }
        }
    }
}