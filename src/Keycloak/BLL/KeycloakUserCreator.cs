namespace HoU.GuildBot.Keycloak.BLL;

internal class KeycloakUserCreator : IKeycloakUserCreator
{
    private readonly IKeycloakUserWriter _keycloakUserWriter;
    private readonly IKeycloakUserReader _keycloakUserReader;
    private readonly ILogger<KeycloakUserCreator> _logger;

    public KeycloakUserCreator(IKeycloakUserWriter keycloakUserWriter,
                               IKeycloakUserReader keycloakUserReader,
                               ILogger<KeycloakUserCreator> logger)
    {
        _keycloakUserWriter = keycloakUserWriter;
        _keycloakUserReader = keycloakUserReader;
        _logger = logger;
    }

    async Task<int> IKeycloakUserCreator.CreateUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                          KeycloakDiscordDiff keycloakDiscordDiff)
    {
        if (!keycloakDiscordDiff.UsersToAdd.Any())
            return 0;

        var newUsers = await _keycloakUserWriter.CreateUsersAsync(keycloakEndpoint,
                                                                  keycloakDiscordDiff.UsersToAdd.Select(m => m.DiscordUser));
        _logger.LogInformation("{Count} new Keycloak users were created", newUsers.Count);
        
        var rolesToAddToNewUsers = newUsers.Join(keycloakDiscordDiff.UsersToAdd,
                                                 kvp => kvp.Key,
                                                 tuple => tuple.DiscordUser.DiscordUserId,
                                                 (kvp,
                                                  tuple) => new
                                                 {
                                                     KeycloakUserId = kvp.Value,
                                                     tuple.KeycloakGroupIds
                                                 })
                                           .ToArray();
        _logger.LogInformation("{Count} roles will be added to the new users", rolesToAddToNewUsers.Length);
        foreach (var newUser in rolesToAddToNewUsers)
            keycloakDiscordDiff.GroupsToAdd.Add(newUser.KeycloakUserId, newUser.KeycloakGroupIds);
        
        return newUsers.Count;
    }
}