namespace HoU.GuildBot.Keycloak.DAL;

internal interface IKeycloakGroupReader
{
    /// <summary>
    /// Queries Keycloak for all valid groups that can be synced to the system.
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <returns>The <see cref="ConfiguredKeycloakGroups"/>, if available, otherwise <b>null</b>.</returns>
    Task<ConfiguredKeycloakGroups?> GetConfiguredGroupsAsync(KeycloakEndpoint endpoint);

    /// <summary>
    /// Queries Keycloak for all members of the <paramref name="groupId"/>.
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <param name="groupId">The Id of the keycloak group to fetch the members for.</param>
    /// <returns>The members of the group, if available, otherwise <b>null</b>.</returns>
    Task<KeycloakUserId[]?> GetGroupMembersAsync(KeycloakEndpoint endpoint,
                                                 KeycloakGroupId groupId);
}