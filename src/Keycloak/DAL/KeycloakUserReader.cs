namespace HoU.GuildBot.Keycloak.DAL;

internal class KeycloakUserReader : KeycloakBaseClient, IKeycloakUserReader
{
    public KeycloakUserReader(IBearerTokenManager<KeycloakBaseClient> bearerTokenManager,
                              IHttpClientFactory httpClientFactory,
                              // ReSharper disable once SuggestBaseTypeForParameterInConstructor
                              ILogger<KeycloakUserReader> logger)
        : base(bearerTokenManager, httpClientFactory, logger)
    {
    }

    private async Task<UserRepresentation[]?> GetAllUserRepresentationsAsync(HttpClient httpClient,
                                                                             KeycloakEndpoint keycloakEndpoint)
    {
        var uri = new Uri(keycloakEndpoint.BaseUrl, $"{keycloakEndpoint.Realm}/users?briefRepresentation=false");
        return await httpClient.PerformAuthorizedRequestAsync(BearerTokenManager,
                                                              keycloakEndpoint,
                                                              InvokeHttpGetRequest(httpClient, uri),
                                                              HandleResponseMessage);

        async Task<UserRepresentation[]?> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            JsonNode? json;
            return (json = await TryGetJsonRootFromResponseAsync(responseMessage, uri)) is null
                       ? null
                       : json.Deserialize<UserRepresentation[]?>();
        }
    }

    private async Task<FederatedIdentityRepresentation?> GetDiscordIdentityProviderIdAsync(HttpClient httpClient,
                                                                                           KeycloakEndpoint endpoint,
                                                                                           KeycloakUserId userId)
    {
        var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{userId}/federated-identity");
        return await httpClient.PerformAuthorizedRequestAsync(BearerTokenManager,
                                                              endpoint,
                                                              InvokeHttpGetRequest(httpClient, uri),
                                                              HandleResponseMessage);

        async Task<FederatedIdentityRepresentation?> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            JsonNode? json;
            if ((json = await TryGetJsonRootFromResponseAsync(responseMessage, uri)) is null)
                return null;

            var identityProviders = json.AsArray();
            return identityProviders.Count == 0
                       ? null
                       : (from identityProvider in identityProviders
                          where identityProvider is not null
                          where identityProvider["identityProvider"]?.GetValue<string?>() == FederatedIdentityRepresentation.DiscordIdentityProviderName
                          select identityProvider.Deserialize<FederatedIdentityRepresentation>())
                      .FirstOrDefault();
        }
    }

    private async Task<KeycloakUserId[]?> GetDisabledUsersForDateAsync(HttpClient httpClient,
                                                                       KeycloakEndpoint endpoint,
                                                                       DateOnly date)
    {
        var dateFilter = $"&q={KnownAttributes.DeleteAfter}:{date:yyyy-MM-dd}";
        var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/?enabled=false{dateFilter}&briefRepresentation=true");

        return await httpClient.PerformAuthorizedRequestAsync(BearerTokenManager,
                                                              endpoint,
                                                              InvokeHttpGetRequest(httpClient, uri),
                                                              HandleResponseMessage);

        async Task<KeycloakUserId[]?> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            JsonNode? json;
            if ((json = await TryGetJsonRootFromResponseAsync(responseMessage, uri)) is null)
                return null;

            var users = json.AsArray();
            if (users.Count == 0)
                return Array.Empty<KeycloakUserId>();

            var result = new List<KeycloakUserId>();

            foreach (var user in users)
            {
                var idValue = user?["id"]?.GetValue<string?>();
                if (idValue is null)
                    continue;

                if (Guid.TryParse(idValue, out var id))
                    result.Add((KeycloakUserId)id);
            }

            return result.ToArray();
        }
    }

    async Task<UserRepresentation[]?> IKeycloakUserReader.GetAllUsersAsync(KeycloakEndpoint keycloakEndpoint)
    {
        var httpClient = GetHttpClient();
        return await GetAllUserRepresentationsAsync(httpClient,
                                                    keycloakEndpoint);
    }

    async Task<FederatedIdentityRepresentation?> IKeycloakUserReader.GetFederatedIdentityAsync(KeycloakEndpoint endpoint,
                                                                                               KeycloakUserId userId)
    {
        var httpClient = GetHttpClient();
        return await GetDiscordIdentityProviderIdAsync(httpClient,
                                                       endpoint,
                                                       userId);
    }

    async Task<KeycloakUserId[]?> IKeycloakUserReader.GetUsersFlaggedForDeletionAsync(KeycloakEndpoint endpoint,
                                                                                      DateOnly date)
    {
        var httpClient = GetHttpClient();
        return await GetDisabledUsersForDateAsync(httpClient, endpoint, date);
    }
}