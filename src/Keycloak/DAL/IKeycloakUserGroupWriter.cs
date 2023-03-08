namespace HoU.GuildBot.Keycloak.DAL;

internal interface IKeycloakUserGroupWriter
{
    Task<int> CreateGroupAssignmentsAsync(KeycloakEndpoint keycloakEndpoint,
                                          IReadOnlyDictionary<KeycloakUserId, KeycloakGroupId[]> groupAssignmentsToCreate);

    Task<int> RemoveGroupAssignmentsAsync(KeycloakEndpoint keycloakEndpoint,
                                          IReadOnlyDictionary<KeycloakUserId, KeycloakGroupId[]> groupAssignmentsToRemove);
}