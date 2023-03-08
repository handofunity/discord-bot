namespace HoU.GuildBot.Keycloak.BLL;

internal interface IKeycloakUserUpdater
{
    Task<(int EnabledUsers, int DisabledUsers, int LoggedOutUsers)> UpdateUserActivationStateAsync(KeycloakEndpoint keycloakEndpoint,
                                                                                                   KeycloakDiscordDiff keycloakDiscordDiff);

    Task<int> UpdateUserDetailsAsync(KeycloakEndpoint keycloakEndpoint,
                                     KeycloakDiscordDiff keycloakDiscordDiff);
}