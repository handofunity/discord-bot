namespace HoU.GuildBot.Shared.DAL;

public interface IKeycloakAccess
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

    /// <summary>
    /// Gets the <see cref="DiscordUserId"/> for the given <see cref="KeycloakUserId"/>.
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <param name="userId">The <see cref="KeycloakUserId"/> to get the <see cref="DiscordUserId"/> for.</param>
    /// <returns>The <see cref="DiscordUserId"/>, if available, otherwise <b>null</b>.</returns>
    Task<DiscordUserId?> GetDiscordUserIdAsync(KeycloakEndpoint endpoint,
                                               KeycloakUserId userId);

    /// <summary>
    /// Sends the <paramref name="diff"/> to Keycloak.
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <param name="diff">The diff that will be sent to Keycloak.</param>
    /// <returns>A <see cref="KeycloakSyncResponse"/>, if the <paramref name="diff"/> was synchronized, otherwise <b>null</b>.</returns>
    Task<KeycloakSyncResponse?> SendDiffAsync(KeycloakEndpoint endpoint,
                                              KeycloakDiscordDiff diff);

    /// <summary>
    /// Queries Keycloak for all users that are flagged for deletion.
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <param name="date">Filter for users that are flagged to be deleted on the given date.</param>
    /// <returns>The users that are flagged for deletion, if available, otherwise <b>null</b>.</returns>
    /// <remarks>If <paramref name="date"/> is set, only users marked to be deleted on that given <paramref name="date"/> will be returned.
    /// If <b>null</b>, all disabled users with any <paramref name="date"/> will be returned.</remarks>
    Task<KeycloakUserId[]?> GetUsersFlaggedForDeletionAsync(KeycloakEndpoint endpoint,
                                                            DateOnly? date);

    /// <summary>
    /// Deletes the users for the given <paramref name="userIds"/>. 
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <param name="userIds">The Ids of the users to permanently delete.</param>
    /// <returns>The amount of successfully deleted users, if possible, otherwise <b>null</b>.</returns>
    Task<int?> DeleteUsersAsync(KeycloakEndpoint endpoint,
                                KeycloakUserId[] userIds);
}