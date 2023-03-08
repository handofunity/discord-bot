namespace HoU.GuildBot.Keycloak.BLL;

internal interface IKeycloakUserCreator
{
    Task<int> CreateUsersAsync(KeycloakEndpoint keycloakEndpoint,
                               KeycloakDiscordDiff keycloakDiscordDiff);
}