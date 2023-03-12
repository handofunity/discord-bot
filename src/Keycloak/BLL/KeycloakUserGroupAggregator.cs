namespace HoU.GuildBot.Keycloak.BLL;

internal class KeycloakUserGroupAggregator : IKeycloakUserGroupAggregator
{
    private readonly IKeycloakGroupReader _keycloakGroupReader;
    private readonly IKeycloakUserReader _keycloakUserReader;

    public KeycloakUserGroupAggregator(IKeycloakGroupReader keycloakGroupReader,
                                       IKeycloakUserReader keycloakUserReader)
    {
        _keycloakGroupReader = keycloakGroupReader;
        _keycloakUserReader = keycloakUserReader;
    }
    
    async Task<KeycloakUserGroupAggregation?> IKeycloakUserGroupAggregator.AggregateCurrentStateAsync(KeycloakEndpoint keycloakEndpoint)
    {
        // Get Keycloak groups (aka roles in Discord)
        var configuredKeycloakGroups = await _keycloakGroupReader.GetConfiguredGroupsAsync(keycloakEndpoint);
        if (configuredKeycloakGroups is null)
            return null;
        
        // Get all Keycloak users
        var keycloakUsers = await _keycloakUserReader.GetAllUsersAsync(keycloakEndpoint);
        if (keycloakUsers is null)
            return null;

        // Get Keycloak group members
        var keycloakGroupMembers = new Dictionary<KeycloakGroupId, KeycloakUserId[]>();
        foreach (var keycloakGroupId in configuredKeycloakGroups.DiscordRoleToKeycloakGroupMapping.Values)
        {
            var members = await _keycloakGroupReader.GetGroupMembersAsync(keycloakEndpoint, keycloakGroupId);
            if (members is not null)
                keycloakGroupMembers.Add(keycloakGroupId, members);
        }

        var fallbackMembers = await _keycloakGroupReader.GetGroupMembersAsync(keycloakEndpoint,
                                                                              configuredKeycloakGroups.FallbackGroupId);
        if (fallbackMembers is not null)
            keycloakGroupMembers.Add(configuredKeycloakGroups.FallbackGroupId, fallbackMembers);

        // Get mapping of KeycloakUserId to DiscordUserId
        foreach (var keycloakUser in keycloakUsers)
        {
            var federatedIdentity = await _keycloakUserReader.GetFederatedIdentityAsync(keycloakEndpoint, keycloakUser.KeycloakUserId);
            if (federatedIdentity is null)
                continue;
            keycloakUser.AddFederatedIdentity(federatedIdentity);
        }

        var userMapping = keycloakUsers.Where(m => m.DiscordUserId != default)
                                       .ToDictionary(m => m.KeycloakUserId, m => m.DiscordUserId);
            
        // Get currently disabled users
        var disabledUsers = keycloakUsers.Where(m => !m.Enabled).Select(m => m.KeycloakUserId).ToArray();

        return new KeycloakUserGroupAggregation(configuredKeycloakGroups,
                                                keycloakUsers,
                                                userMapping,
                                                keycloakGroupMembers,
                                                disabledUsers);
    }
}