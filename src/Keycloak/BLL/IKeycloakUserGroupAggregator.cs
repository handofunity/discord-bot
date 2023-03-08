namespace HoU.GuildBot.Keycloak.BLL;

internal interface IKeycloakUserGroupAggregator
{
    Task<KeycloakUserGroupAggregation?> AggregateCurrentStateAsync(KeycloakEndpoint keycloakEndpoint);
}