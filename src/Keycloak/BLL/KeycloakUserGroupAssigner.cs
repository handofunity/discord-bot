namespace HoU.GuildBot.Keycloak.BLL;

internal class KeycloakUserGroupAssigner : IKeycloakUserGroupAssigner
{
    private readonly IKeycloakUserGroupWriter _keycloakUserGroupWriter;

    public KeycloakUserGroupAssigner(IKeycloakUserGroupWriter keycloakUserGroupWriter)
    {
        _keycloakUserGroupWriter = keycloakUserGroupWriter;
    }
    
    async Task<(int AssignedUserGroupRelationships, int UnassignedUserGroupRelationships)> IKeycloakUserGroupAssigner.
        UpdateUserGroupAssignments(KeycloakEndpoint keycloakEndpoint,
                                   KeycloakDiscordDiff keycloakDiscordDiff)
    {
        var assignedUserGroupRelationships = 0;
        if (keycloakDiscordDiff.GroupsToAdd.Any())
        {
            assignedUserGroupRelationships = await _keycloakUserGroupWriter.CreateGroupAssignmentsAsync(keycloakEndpoint,
                                                 keycloakDiscordDiff.GroupsToAdd);
        }

        var unassignedUserGroupRelationships = 0;
        if (keycloakDiscordDiff.GroupsToRemove.Any())
        {
            unassignedUserGroupRelationships = await _keycloakUserGroupWriter.RemoveGroupAssignmentsAsync(keycloakEndpoint,
                                                   keycloakDiscordDiff.GroupsToRemove);
        }

        return (assignedUserGroupRelationships, unassignedUserGroupRelationships);
    }
}