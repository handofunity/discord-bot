namespace HoU.GuildBot.Keycloak.BLL;

internal interface IKeycloakUserCleaner
{
    Task DeleteFlaggedUsersAsync(KeycloakEndpoint keycloakEndpoint);
}