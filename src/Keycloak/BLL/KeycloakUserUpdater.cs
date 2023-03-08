namespace HoU.GuildBot.Keycloak.BLL;

internal class KeycloakUserUpdater : IKeycloakUserUpdater
{
    private readonly IKeycloakUserWriter _keycloakUserWriter;

    public KeycloakUserUpdater(IKeycloakUserWriter keycloakUserWriter)
    {
        _keycloakUserWriter = keycloakUserWriter;
    }
    
    async Task<(int EnabledUsers, int DisabledUsers, int LoggedOutUsers)> IKeycloakUserUpdater.UpdateUserActivationStateAsync(
        KeycloakEndpoint keycloakEndpoint,
        KeycloakDiscordDiff keycloakDiscordDiff)
    {
        var enabledUsers = 0;
        if (keycloakDiscordDiff.UsersToEnable.Any())
        {
            enabledUsers = await _keycloakUserWriter.UnlockUsersAsync(keycloakEndpoint, keycloakDiscordDiff.UsersToEnable);
        }
        
        var disabledUsers = 0;
        var loggedOutUsers = 0;
        // ReSharper disable once InvertIf
        if (keycloakDiscordDiff.UsersToDisable.Any())
        {
            var userIds = keycloakDiscordDiff.UsersToDisable.Select(m => m.KeycloakUserId).ToArray();
            disabledUsers = await _keycloakUserWriter.LockUsersAsync(keycloakEndpoint, keycloakDiscordDiff.UsersToDisable);
            loggedOutUsers = await _keycloakUserWriter.LogoutUsersAsync(keycloakEndpoint, userIds);
        }

        return (enabledUsers, disabledUsers, loggedOutUsers);
    }

    async Task<int> IKeycloakUserUpdater.UpdateUserDetailsAsync(KeycloakEndpoint keycloakEndpoint,
                                                                KeycloakDiscordDiff keycloakDiscordDiff)
    {
        var updatedUsers = 0;
        if (keycloakDiscordDiff.UsersWithDifferentProperties.Any())
            updatedUsers += await _keycloakUserWriter.UpdateUserPropertiesAsync(keycloakEndpoint,
                                                                                keycloakDiscordDiff.UsersWithDifferentProperties);

        if (keycloakDiscordDiff.UsersWithDifferentIdentityData.Any())
            updatedUsers += await _keycloakUserWriter.UpdateIdentityProvidersAsync(keycloakEndpoint,
                                                                                   keycloakDiscordDiff.UsersWithDifferentIdentityData);

        return updatedUsers;
    }
}