namespace HoU.GuildBot.Keycloak.DAL;

internal interface IKeycloakUserWriter
{
    Task<Dictionary<DiscordUserId, KeycloakUserId>> CreateUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                                     IEnumerable<UserModel> users);

    Task<int> UpdateUserPropertiesAsync(KeycloakEndpoint keycloakEndpoint,
                                        IEnumerable<(UserModel DiscordState, UserRepresentation KeycloakState)> users);

    Task<int> UpdateIdentityProvidersAsync(KeycloakEndpoint keycloakEndpoint,
                                           IReadOnlyDictionary<KeycloakUserId, UserModel> users);

    Task<int> UnlockUsersAsync(KeycloakEndpoint keycloakEndpoint,
                               IEnumerable<UserRepresentation> users);

    Task<int> LockUsersAsync(KeycloakEndpoint keycloakEndpoint,
                             IEnumerable<UserRepresentation> users);

    Task<int> LogoutUsersAsync(KeycloakEndpoint keycloakEndpoint,
                               KeycloakUserId[] users);
}