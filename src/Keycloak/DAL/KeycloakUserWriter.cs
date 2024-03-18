namespace HoU.GuildBot.Keycloak.DAL;

internal class KeycloakUserWriter : KeycloakBaseClient, IKeycloakUserWriter
{

    public KeycloakUserWriter(IBearerTokenManager bearerTokenManager,
                              IHttpClientFactory httpClientFactory,
                              // ReSharper disable once SuggestBaseTypeForParameterInConstructor
                              ILogger<KeycloakUserWriter> logger)
        : base(bearerTokenManager, httpClientFactory, logger)
    {
    }

    private async Task<Dictionary<DiscordUserId,KeycloakUserId>> CreateDiscordUsersAsync(HttpClient httpClient,
                                                                                         KeycloakEndpoint keycloakEndpoint,
                                                                                         IEnumerable<UserModel> users)
    {
        var result = new Dictionary<DiscordUserId, KeycloakUserId>();
        var uri = new Uri(keycloakEndpoint.BaseUrl, $"{keycloakEndpoint.Realm}/users");

        foreach (var user in users)
        {
            var request = new UserRepresentation(user);
            Logger.LogTrace("Creating new Keycloak user for {Username} ({DiscordUserId}) ...",
                            user.Username,
                            user.DiscordUserId);
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, uri);// add the new code
            requestMessage.Content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
            var newKeycloakUserId = await httpClient.PerformAuthorizedRequestAsync(requestMessage,
                                                                                   BearerTokenManager,
                                                                                   keycloakEndpoint,
                                                                                   HandleResponseMessage);
            if (newKeycloakUserId is not null)
            {
                Logger.LogTrace("Created new Keycloak user {UserId} for {Username} ({DiscordUserId})",
                                newKeycloakUserId.Value,
                                user.Username,
                                user.DiscordUserId);
                result.Add(user.DiscordUserId, newKeycloakUserId.Value);
            }
            else
            {
                Logger.LogWarning("No new Keycloak user was created for {Username} ({DiscordUserId})",
                                  user.Username,
                                  user.DiscordUserId);
            }
        }

        return result;

        Task<KeycloakUserId?> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            if (responseMessage is null)
                return Task.FromResult<KeycloakUserId?>(null);

            if (responseMessage.StatusCode == HttpStatusCode.Created)
                // Format: https://DOMAIN/admin/realms/REALM/users/GUID
                return Task.FromResult((KeycloakUserId?)Guid.Parse(responseMessage.Headers.Location!.Segments.Last()));

            Logger.LogRequestError(uri.Host,
                                   uri.PathAndQuery,
                                   $"Creating Keycloak user failed: {responseMessage.StatusCode}");
            return Task.FromResult<KeycloakUserId?>(null);
        }
    }

    private async Task<int> UpdateUserAsync(HttpClient httpClient,
                                            KeycloakEndpoint endpoint,
                                            Dictionary<KeycloakUserId, UserUpdateRepresentation> users)
    {
        var updatedUsers = 0;

        foreach (var user in users)
        {
            var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{user.Key}");
            var json = user.Value.ToJson().ToJsonString();
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            var updated = await httpClient.PerformAuthorizedRequestAsync(request,
                                                                         BearerTokenManager,
                                                                         endpoint,
                                                                         HandleResponseMessage);
            if (updated)
                updatedUsers++;
        }

        return updatedUsers;

        Task<bool> HandleResponseMessage(HttpResponseMessage? responseMessage) =>
            Task.FromResult(responseMessage is { StatusCode: HttpStatusCode.NoContent });
    }

    private async Task<int> DisableUsersAsync(HttpClient httpClient,
                                              KeycloakEndpoint endpoint,
                                              IEnumerable<UserRepresentation> users)
    {
        var inSixMonths = DateTime.UtcNow.AddMonths(6).ToString("yyyy-MM-dd");
        var usersUpdateRep = users.ToDictionary(m => m.KeycloakUserId, m => m.AsUpdateRepresentation());

        foreach (var user in usersUpdateRep)
        {
            user.Value.Enabled = false;
            user.Value.Attributes.SetDeleteAfter(inSixMonths);
        }

        return await UpdateUserAsync(httpClient, endpoint, usersUpdateRep);
    }

    private async Task<int> EnableUsersAsync(HttpClient httpClient,
                                             KeycloakEndpoint endpoint,
                                             IEnumerable<UserRepresentation> users)
    {
        var usersUpdateRep = users.ToDictionary(m => m.KeycloakUserId, m => m.AsUpdateRepresentation());

        foreach (var user in usersUpdateRep)
        {
            user.Value.Enabled = true;
            user.Value.Attributes.SetDeleteAfter(null);
        }

        return await UpdateUserAsync(httpClient, endpoint, usersUpdateRep);
    }

    private async Task<int> LogoutUsersAsync(HttpClient httpClient,
                                             KeycloakEndpoint endpoint,
                                             IEnumerable<KeycloakUserId> usersToDisable)
    {
        var loggedOutUsers = 0;

        foreach (var userId in usersToDisable)
        {
            var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{userId}/logout");
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var loggedOut = await httpClient.PerformAuthorizedRequestAsync(request,
                                                                           BearerTokenManager,
                                                                           endpoint,
                                                                           HandleResponseMessage);
            if (loggedOut)
                loggedOutUsers++;
        }

        return loggedOutUsers;

        Task<bool> HandleResponseMessage(HttpResponseMessage? responseMessage) =>
            Task.FromResult(responseMessage is { StatusCode: HttpStatusCode.NoContent });
    }

    private async Task<int> UpdateDifferentUserPropertiesAsync(HttpClient httpClient,
                                                               KeycloakEndpoint keycloakEndpoint,
                                                               IEnumerable<(UserModel DiscordState, UserRepresentation KeycloakState)> users)
    {
        var usersUpdateRep = new Dictionary<KeycloakUserId, UserUpdateRepresentation>();
        
        foreach (var user in users)
        {
            var updateRep = user.KeycloakState.AsUpdateRepresentation();

            updateRep.FirstName = user.DiscordState.Username;
            updateRep.Attributes.SetDiscordAvatarId(user.DiscordState.AvatarId);
            updateRep.Attributes.SetDiscordNickname(user.DiscordState.Nickname);
            
            usersUpdateRep.Add(user.KeycloakState.KeycloakUserId, updateRep);
        }

        return await UpdateUserAsync(httpClient, keycloakEndpoint, usersUpdateRep);
    }

    private async Task<int> DropAndCreateFederatedIdentitiesAsync(HttpClient httpClient,
                                                                  KeycloakEndpoint keycloakEndpoint,
                                                                  IReadOnlyDictionary<KeycloakUserId,UserModel> users)
    {
        var updated = 0;

        foreach (var (keycloakUserId, discordState) in users)
        {
            var uri = new Uri(keycloakEndpoint.BaseUrl, $"{keycloakEndpoint.Realm}/users/{keycloakUserId}/federated-identity/{FederatedIdentityRepresentation.DiscordIdentityProviderName}");
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            var (removed, statusCode, error) = await httpClient.PerformAuthorizedRequestAsync(request,
                                                        BearerTokenManager,
                                                        keycloakEndpoint,
                                                        HandleResponseMessage);
            if (!removed)
            {
                Logger.LogError("Failed to remove Discord identity provider for {User}: {StatusCode} {Error}",
                                keycloakUserId,
                                statusCode,
                                error);
            }
            else
            {
                Logger.LogInformation("Removed the Discord identity provider for {User}", keycloakUserId);
                var federatedIdentity = new FederatedIdentityRepresentation(discordState.DiscordUserId,
                                                                            discordState.Username);
                request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new StringContent(JsonSerializer.Serialize(federatedIdentity), Encoding.UTF8, "application/json");
                (var added, statusCode, error) = await httpClient.PerformAuthorizedRequestAsync(request,
                                                          BearerTokenManager,
                                                          keycloakEndpoint,
                                                          HandleResponseMessage);
                if (added)
                {
                    updated++;
                    Logger.LogInformation("Added the Discord identity provider for {User} to be {@FederatedIdentity}",
                                          keycloakUserId,
                                          federatedIdentity);
                }
                else
                {
                    Logger.LogError("Failed to re-create Discord identity provider for {User}: {StatusCode} {Error}",
                                    keycloakUserId,
                                    statusCode,
                                    error);
                }
            }
        }
        
        return updated;

        async Task<(bool Success, HttpStatusCode StatusCodes, string? Error)> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            if (responseMessage.StatusCode == HttpStatusCode.NoContent)
                return (true, responseMessage.StatusCode, null);

            try
            {
                var error = await responseMessage.Content.ReadAsStringAsync();
                return (false, responseMessage.StatusCode, error);
            }
            catch (Exception e)
            {
                return (false, responseMessage.StatusCode, e.ToString());
            }
        }
    }

    async Task<Dictionary<DiscordUserId, KeycloakUserId>> IKeycloakUserWriter.CreateUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                                                               IEnumerable<UserModel> users)
    {
        var httpClient = GetHttpClient();
        return await CreateDiscordUsersAsync(httpClient, keycloakEndpoint, users);
    }

    async Task<int> IKeycloakUserWriter.UpdateUserPropertiesAsync(KeycloakEndpoint keycloakEndpoint,
                                                                  IEnumerable<(UserModel DiscordState, UserRepresentation KeycloakState)> users)
    {
        var httpClient = GetHttpClient();
        return await UpdateDifferentUserPropertiesAsync(httpClient, keycloakEndpoint, users);
    }

    async Task<int> IKeycloakUserWriter.UpdateIdentityProvidersAsync(KeycloakEndpoint keycloakEndpoint,
                                                                     IReadOnlyDictionary<KeycloakUserId, UserModel> users)
    {
        var httpClient = GetHttpClient();
        return await DropAndCreateFederatedIdentitiesAsync(httpClient, keycloakEndpoint, users);
    }

    async Task<int> IKeycloakUserWriter.LockUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                       IEnumerable<UserRepresentation> users)
    {
        var httpClient = GetHttpClient();
        return await DisableUsersAsync(httpClient, keycloakEndpoint, users);
    }

    async Task<int> IKeycloakUserWriter.LogoutUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                         KeycloakUserId[] users)
    {
        var httpClient = GetHttpClient();
        return await LogoutUsersAsync(httpClient, keycloakEndpoint, users);
    }

    async Task<int> IKeycloakUserWriter.UnlockUsersAsync(KeycloakEndpoint keycloakEndpoint,
                                                         IEnumerable<UserRepresentation> users)
    {
        var httpClient = GetHttpClient();
        return await EnableUsersAsync(httpClient, keycloakEndpoint, users);
    }
}