namespace HoU.GuildBot.Keycloak.BLL;

internal interface IKeycloakUserGroupAssigner
{
    Task<(int AssignedUserGroupRelationships, int UnassignedUserGroupRelationships)> UpdateUserGroupAssignments(
        KeycloakEndpoint keycloakEndpoint,
        KeycloakDiscordDiff keycloakDiscordDiff);
}