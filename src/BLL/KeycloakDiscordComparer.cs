namespace HoU.GuildBot.BLL;

public class KeycloakDiscordComparer : IKeycloakDiscordComparer
{
    private static KeycloakGroupId[] MapDiscordRolesToKeycloakGroups(UserModel userModel,
                                                                     ConfiguredKeycloakGroups configuredKeycloakGroups)
    {
        var keycloakGroupIds = userModel.Roles
                                        .Join(configuredKeycloakGroups.DiscordRoleToKeycloakGroupMapping,
                                              roleId => roleId,
                                              kvp => kvp.Key,
                                              (_,
                                               kvp) => kvp.Value)
                                        .ToArray();
        return keycloakGroupIds.Any()
                   ? keycloakGroupIds
                   : new[] { configuredKeycloakGroups.FallbackGroupId };
    }

    private static (UserModel DiscordUser, KeycloakGroupId[] KeycloakGroupIds)[] GetUsersToAdd(UserModel[] discordUsers,
        ConfiguredKeycloakGroups configuredKeycloakGroups,
        IEnumerable<DiscordUserId> discordUserIds,
        IEnumerable<DiscordUserId> discordUserIdsInKeycloak)
    {
        var missingDiscordUserIds = discordUserIds.Except(discordUserIdsInKeycloak).ToArray();
        return missingDiscordUserIds.Select(missingDiscordUserId => discordUsers.Single(m => m.DiscordUserId == missingDiscordUserId))
                                    .Select(missingDiscordUser =>
                                                (missingDiscordUser,
                                                 MapDiscordRolesToKeycloakGroups(missingDiscordUser, configuredKeycloakGroups)))
                                    .ToArray();
    }

    private static KeycloakUserId[] GetUsersToDisable(IEnumerable<DiscordUserId> discordUserIds,
                                                      IEnumerable<DiscordUserId> discordUserIdsInKeycloak,
                                                      Dictionary<KeycloakUserId, DiscordUserId> keycloakUsers,
                                                      IEnumerable<KeycloakUserId> disabledUserIds)
    {
        var obsoleteDiscordUserIds = discordUserIdsInKeycloak.Except(discordUserIds).ToArray();
        return obsoleteDiscordUserIds.Select(obsoleteDiscordUserId => keycloakUsers.Single(m => m.Value == obsoleteDiscordUserId).Key)
                                     .Except(disabledUserIds)
                                     .ToArray();
    }

    private static KeycloakUserId[] GetUsersToEnable(IEnumerable<DiscordUserId> discordUserIds,
                                                     Dictionary<KeycloakUserId,DiscordUserId> keycloakUsers,
                                                     IEnumerable<KeycloakUserId> disabledUserIds) =>
        disabledUserIds.Join(keycloakUsers,
                             disabledId => disabledId,
                             mapping => mapping.Key,
                             (_,
                              mapping) => mapping)
                       .Join(discordUserIds,
                             mapping => mapping.Value,
                             discordId => discordId,
                             (mapping,
                              _) => mapping.Key)
                       .ToArray();

    private static (Dictionary<KeycloakUserId, KeycloakGroupId[]> GroupsToAdd, Dictionary<KeycloakUserId, KeycloakGroupId[]> GroupsToRemove)
        GetGroupsDiff(Dictionary<KeycloakUserId, KeycloakGroupId[]> groupMembershipsInDiscord,
                      Dictionary<KeycloakUserId, KeycloakGroupId[]> groupMembershipsInKeycloak)
    {
        var groupsToAdd = new Dictionary<KeycloakUserId, KeycloakGroupId[]>();
        var groupsToRemove = new Dictionary<KeycloakUserId, KeycloakGroupId[]>();

        var groupDiffByUser = groupMembershipsInDiscord.Join(groupMembershipsInKeycloak,
                                                             discord => discord.Key,
                                                             keycloak => keycloak.Key,
                                                             (discord,
                                                              keycloak) => new
                                                             {
                                                                 KeycloakUserId = discord.Key,
                                                                 GroupsToAdd = discord.Value.Except(keycloak.Value).ToArray(),
                                                                 GroupsToRemove = keycloak.Value.Except(discord.Value).ToArray()
                                                             });
        foreach (var diff in groupDiffByUser)
        {
            if (diff.GroupsToAdd.Any())
                groupsToAdd.Add(diff.KeycloakUserId, diff.GroupsToAdd);
            if (diff.GroupsToRemove.Any())
                groupsToRemove.Add(diff.KeycloakUserId, diff.GroupsToRemove);
        }
        
        return (groupsToAdd, groupsToRemove);
    }

    KeycloakDiscordDiff IKeycloakDiscordComparer.GetDiff(UserModel[] discordUsers,
                                                         Dictionary<KeycloakUserId, DiscordUserId> keycloakUsers,
                                                         ConfiguredKeycloakGroups configuredKeycloakGroups,
                                                         Dictionary<KeycloakGroupId, KeycloakUserId[]> keycloakGroupMembers,
                                                         KeycloakUserId[] disabledUserIds)
    {
        // Prepare
        var discordUserIds = discordUsers.Select(m => m.DiscordUserId).ToArray();
        var discordUserIdsInKeycloak = keycloakUsers.Values.ToArray();
        var groupMembershipsInDiscord = discordUsers.Where(m => discordUserIdsInKeycloak.Contains(m.DiscordUserId))
                                                       .Join(keycloakUsers,
                                                             userModel => userModel.DiscordUserId,
                                                             kvp => kvp.Value,
                                                             (userModel,
                                                              kvp) => new
                                                             {
                                                                 KeycloakUserId = kvp.Key,
                                                                 KeycloakGroupIds =
                                                                     MapDiscordRolesToKeycloakGroups(userModel, configuredKeycloakGroups)
                                                             })
                                                       .ToDictionary(m => m.KeycloakUserId, m => m.KeycloakGroupIds);
        var groupMembershipsInKeycloak = keycloakGroupMembers.SelectMany(m => m.Value,
                                                                         (groupMembers,
                                                                          keycloakUserId) => new
                                                                         {
                                                                             KeycloakUserId = keycloakUserId,
                                                                             KeycloakGroupId = groupMembers.Key
                                                                         })
                                                             .GroupBy(m => m.KeycloakUserId)
                                                             .ToDictionary(m => m.Key, m => m.Select(x => x.KeycloakGroupId).ToArray());
        
        // Compute diff to add or remove complete users
        var usersToAdd = GetUsersToAdd(discordUsers, configuredKeycloakGroups, discordUserIds, discordUserIdsInKeycloak);
        var usersToDisable = GetUsersToDisable(discordUserIds, discordUserIdsInKeycloak, keycloakUsers, disabledUserIds);
        var usersToEnable = GetUsersToEnable(discordUserIds, keycloakUsers, disabledUserIds);
        
        // Compute diff of groups for existing users
        var (groupsToAdd, groupsToRemove) = GetGroupsDiff(groupMembershipsInDiscord, groupMembershipsInKeycloak);

        return new KeycloakDiscordDiff(usersToAdd, usersToDisable, usersToEnable, groupsToAdd, groupsToRemove);
    }
}