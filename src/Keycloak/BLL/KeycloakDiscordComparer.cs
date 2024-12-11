namespace HoU.GuildBot.Keycloak.BLL;

internal class KeycloakDiscordComparer : IKeycloakDiscordComparer
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
                   : [configuredKeycloakGroups.FallbackGroupId];
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

    private static UserRepresentation[] GetUsersToDisable(IEnumerable<DiscordUserId> discordUserIds,
                                                          IEnumerable<DiscordUserId> discordUserIdsInKeycloak,
                                                          IReadOnlyDictionary<KeycloakUserId, DiscordUserId> userMapping,
                                                          IEnumerable<KeycloakUserId> disabledUserIds,
                                                          IEnumerable<UserRepresentation> keycloakUsers)
    {
        var obsoleteDiscordUserIds = discordUserIdsInKeycloak
            .Where(m => m != DiscordUserId.Unknown)
            .Except(discordUserIds)
            .ToArray();
        var userIdsToDisable = obsoleteDiscordUserIds
            .Select(obsoleteDiscordUserId => userMapping.Single(m => m.Value == obsoleteDiscordUserId).Key)
            .Except(disabledUserIds)
            .ToList();

        userIdsToDisable.AddRange(userMapping.Where(m => m.Value == DiscordUserId.Unknown)
            .Select(m => m.Key));
        userIdsToDisable = userIdsToDisable.Distinct().ToList();

        return userIdsToDisable.Join(keycloakUsers,
                id => id,
                userRep => userRep.KeycloakUserId,
                (_,
                userRep) => userRep)
            .Where(m => m.Enabled)
            .ToArray();
    }

    private static UserRepresentation[] GetUsersToEnable(IEnumerable<DiscordUserId> discordUserIds,
                                                         IReadOnlyDictionary<KeycloakUserId, DiscordUserId> userMapping,
                                                         IEnumerable<KeycloakUserId> disabledUserIds,
                                                         IEnumerable<UserRepresentation> keycloakUsers) =>
        disabledUserIds.Join(userMapping,
                             disabledId => disabledId,
                             mapping => mapping.Key,
                             (_,
                              mapping) => mapping)
                       .Join(discordUserIds,
                             mapping => mapping.Value,
                             discordId => discordId,
                             (mapping,
                              _) => mapping.Key)
                       .Join(keycloakUsers,
                             id => id,
                             userRep => userRep.KeycloakUserId,
                             (_,
                              userRep) => userRep)
                       .ToArray();

    private static IReadOnlyList<(UserModel DiscordState, UserRepresentation KeycloakState)> GetUsersWithDifferentProperties(
        IEnumerable<(UserModel DiscordState, UserRepresentation KeycloakState)> matchedUsers)
    {
        var result = new List<(UserModel DiscordState, UserRepresentation KeycloakState)>();
        foreach (var matchedUser in matchedUsers)
        {
            var keycloakAvatarId = matchedUser.KeycloakState.Attributes?.DiscordAvatarId?.FirstOrDefault();
            if (matchedUser.DiscordState.AvatarId != keycloakAvatarId)
            {
                result.Add(matchedUser);
                continue;
            }

            if (matchedUser.DiscordState.Nickname != matchedUser.KeycloakState.LastName)
            {
                result.Add(matchedUser);
                continue;
            }

            if (matchedUser.DiscordState.GlobalName != matchedUser.KeycloakState.FirstName)
            {
                result.Add(matchedUser);
                continue;
            }

            if (matchedUser.DiscordState.Username.ToLower() != matchedUser.KeycloakState.Username?.ToLower())
            {
                result.Add(matchedUser);
            }
        }

        return result;
    }

    private static IReadOnlyDictionary<KeycloakUserId, UserModel> GetUsersWithDifferentIdentityData(
        IEnumerable<(UserModel DiscordState, UserRepresentation KeycloakState)> matchedUsers) =>
        (from matchedUser in matchedUsers
         let discordUserName = matchedUser.DiscordState.Username
         let keycloakIdentityUserName = matchedUser.KeycloakState.FederatedIdentities?.SingleOrDefault()?.Username
         where !string.Equals(discordUserName, keycloakIdentityUserName, StringComparison.InvariantCultureIgnoreCase)
         select matchedUser)
       .ToDictionary(matchedUser => matchedUser.KeycloakState.KeycloakUserId, matchedUser => matchedUser.DiscordState);

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

    KeycloakDiscordDiff IKeycloakDiscordComparer.GetDiff(KeycloakUserGroupAggregation keycloakState,
                                                         UserModel[] discordUsers)
    {
        // Prepare
        var discordUserIds = discordUsers.Select(m => m.DiscordUserId).ToArray();
        var discordUserIdsInKeycloak = keycloakState.KeycloakToDiscordUserMapping.Values.ToArray();
        var groupMembershipsInDiscord = discordUsers.Where(m => discordUserIdsInKeycloak.Contains(m.DiscordUserId))
                                                    .Join(keycloakState.KeycloakToDiscordUserMapping,
                                                          userModel => userModel.DiscordUserId,
                                                          kvp => kvp.Value,
                                                          (userModel,
                                                           kvp) => new
                                                          {
                                                              KeycloakUserId = kvp.Key,
                                                              KeycloakGroupIds = MapDiscordRolesToKeycloakGroups(userModel, keycloakState.ConfiguredKeycloakGroups)
                                                          })
                                                    .ToDictionary(m => m.KeycloakUserId, m => m.KeycloakGroupIds);
        var groupMembershipsInKeycloak = keycloakState.KeycloakGroupMembers.SelectMany(m => m.Value,
                                                                                       (groupMembers,
                                                                                        keycloakUserId) => new
                                                                                       {
                                                                                           KeycloakUserId = keycloakUserId,
                                                                                           KeycloakGroupId = groupMembers.Key
                                                                                       })
                                                      .GroupBy(m => m.KeycloakUserId)
                                                      .ToDictionary(m => m.Key, m => m.Select(x => x.KeycloakGroupId).ToArray());

        // Compute diff to add or remove complete users
        var usersToAdd = GetUsersToAdd(discordUsers, keycloakState.ConfiguredKeycloakGroups, discordUserIds, discordUserIdsInKeycloak);
        var usersToDisable = GetUsersToDisable(discordUserIds,
                                               discordUserIdsInKeycloak,
                                               keycloakState.KeycloakToDiscordUserMapping,
                                               keycloakState.DisabledUserIds,
                                               keycloakState.AllKeycloakUsers);
        var usersToEnable = GetUsersToEnable(discordUserIds,
                                             keycloakState.KeycloakToDiscordUserMapping,
                                             keycloakState.DisabledUserIds,
                                             keycloakState.AllKeycloakUsers);

        // Compute diff to update users
        var matchedUsers = discordUsers
                          .Join(keycloakState.AllKeycloakUsers,
                                  discordUser => discordUser.DiscordUserId,
                                  userRep => userRep.DiscordUserId,
                                  (discordUser,
                                   userRep) => new ValueTuple<UserModel, UserRepresentation>(discordUser, userRep))
                          .ToArray();
        var usersWithDifferentProperties = GetUsersWithDifferentProperties(matchedUsers);
        var usersWithDifferentIdentityData = GetUsersWithDifferentIdentityData(matchedUsers);

        // Compute diff of groups for existing users
        var (groupsToAdd, groupsToRemove) = GetGroupsDiff(groupMembershipsInDiscord, groupMembershipsInKeycloak);

        return new KeycloakDiscordDiff(usersToAdd,
                                       usersToDisable,
                                       usersToEnable,
                                       usersWithDifferentProperties,
                                       usersWithDifferentIdentityData,
                                       groupsToAdd,
                                       groupsToRemove);
    }
}