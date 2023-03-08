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
        var userMapping = new Dictionary<KeycloakUserId, DiscordUserId>();
        var distinctKeycloakUserIds = keycloakGroupMembers.Values.SelectMany(m => m).Distinct();
        foreach (var keycloakUserId in distinctKeycloakUserIds)
        {
            var federatedIdentity = await _keycloakUserReader.GetFederatedIdentityAsync(keycloakEndpoint, keycloakUserId);
            if (federatedIdentity is not null)
                userMapping.Add(keycloakUserId, federatedIdentity.DiscordUserId);
        }
            
        // Get currently disabled users
        var disabledUsers = keycloakUsers.Where(m => !m.Enabled).Select(m => m.KeycloakUserId).ToArray();

        return new KeycloakUserGroupAggregation(configuredKeycloakGroups,
                                                keycloakUsers,
                                                userMapping,
                                                keycloakGroupMembers,
                                                disabledUsers);
    }
}