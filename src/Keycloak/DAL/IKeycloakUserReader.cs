namespace HoU.GuildBot.Keycloak.DAL;

internal interface IKeycloakUserReader
{
    /// <summary>
    /// Gets all <see cref="UserRepresentation"/>s for the given <paramref name="keycloakEndpoint"/>.
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <returns>All current <see cref="UserRepresentation"/>s.</returns>
    Task<UserRepresentation[]?> GetAllUsersAsync(KeycloakEndpoint keycloakEndpoint);

    /// <summary>
    /// Gets the <see cref="FederatedIdentityRepresentation"/> for the given <see cref="KeycloakUserId"/>.
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <param name="userId">The <see cref="KeycloakUserId"/> to get the <see cref="DiscordUserId"/> for.</param>
    /// <returns>The <see cref="FederatedIdentityRepresentation"/>, if available, otherwise <b>null</b>.</returns>
    Task<FederatedIdentityRepresentation?> GetFederatedIdentityAsync(KeycloakEndpoint endpoint,
                                                                     KeycloakUserId userId);

    /// <summary>
    /// Queries Keycloak for all users that are flagged for deletion.
    /// </summary>
    /// <param name="endpoint">The data used to sync with Keycloak.</param>
    /// <param name="date">Filter for users that are flagged to be deleted on the given date.</param>
    /// <returns>The users that are flagged for deletion, if available, otherwise <b>null</b>.</returns>
    Task<KeycloakUserId[]?> GetUsersFlaggedForDeletionAsync(KeycloakEndpoint endpoint,
                                                            DateOnly date);
}