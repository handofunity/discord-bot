namespace HoU.GuildBot.Keycloak.BLL;

internal class KeycloakUserCreator : IKeycloakUserCreator
{
    private readonly IKeycloakUserWriter _keycloakUserWriter;

    public KeycloakUserCreator(IKeycloakUserWriter keycloakUserWriter)
    {
        _keycloakUserWriter = keycloakUserWriter;
    }

    async Task<int> IKeycloakUserCreator.CreateUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                          KeycloakDiscordDiff keycloakDiscordDiff)
    {
        if (!keycloakDiscordDiff.UsersToAdd.Any())
            return 0;

        var newUsers = await _keycloakUserWriter.CreateUsersAsync(keycloakEndpoint,
                                                                  keycloakDiscordDiff.UsersToAdd.Select(m => m.DiscordUser));
        var rolesToAddToNewUsers = newUsers.Join(keycloakDiscordDiff.UsersToAdd,
                                                 kvp => kvp.Key,
                                                 tuple => tuple.DiscordUser.DiscordUserId,
                                                 (kvp,
                                                  tuple) => new
                                                 {
                                                     KeycloakUserId = kvp.Value,
                                                     tuple.KeycloakGroupIds
                                                 });
        foreach (var newUser in rolesToAddToNewUsers)
            keycloakDiscordDiff.GroupsToAdd.Add(newUser.KeycloakUserId, newUser.KeycloakGroupIds);

        return newUsers.Count;
    }
}