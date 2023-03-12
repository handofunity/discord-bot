namespace HoU.GuildBot.Keycloak.DAL;

internal class KeycloakUserEraser : KeycloakBaseClient, IKeycloakUserEraser
{
    public KeycloakUserEraser(IBearerTokenManager<KeycloakBaseClient> bearerTokenManager,
                              IHttpClientFactory httpClientFactory,
                              // ReSharper disable once SuggestBaseTypeForParameterInConstructor
                              ILogger<KeycloakUserEraser> logger)
        : base(bearerTokenManager, httpClientFactory, logger)
    {
    }

    private async Task<int> DeleteUsersByIdAsync(HttpClient httpClient,
                                                 KeycloakEndpoint endpoint,
                                                 IEnumerable<KeycloakUserId> userIds)
    {
        var deletedCount = 0;

        foreach (var userId in userIds)
        {
            var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{userId}");
            var deleted = await httpClient.PerformAuthorizedRequestAsync(BearerTokenManager,
                                                                         endpoint,
                                                                         InvokeHttpDeleteRequest(httpClient, uri),
                                                                         HandleResponseMessage);
            if (deleted)
                deletedCount++;
        }

        return deletedCount;

        Task<bool> HandleResponseMessage(HttpResponseMessage? responseMessage) =>
            Task.FromResult(responseMessage is { StatusCode: HttpStatusCode.NoContent });
    }

    async Task<int> IKeycloakUserEraser.DeleteUsersAsync(KeycloakEndpoint endpoint,
                                                         IEnumerable<KeycloakUserId> userIds)
    {
        var httpClient = GetHttpClient();
        return await DeleteUsersByIdAsync(httpClient, endpoint, userIds);
    }
}