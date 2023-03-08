namespace HoU.GuildBot.Keycloak.DAL;

internal interface IKeycloakUserEraser
{
    /// <summary>
    /// Deletes the users for the given <paramref name="userIds"/>. 
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <param name="userIds">The Ids of the users to permanently delete.</param>
    /// <returns>The amount of successfully deleted users.</returns>
    Task<int> DeleteUsersAsync(KeycloakEndpoint endpoint,
                               IEnumerable<KeycloakUserId> userIds);
}