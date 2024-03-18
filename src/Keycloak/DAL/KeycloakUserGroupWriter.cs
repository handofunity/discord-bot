namespace HoU.GuildBot.Keycloak.DAL;

internal class KeycloakUserGroupWriter : KeycloakBaseClient, IKeycloakUserGroupWriter
{
    public KeycloakUserGroupWriter(IBearerTokenManager bearerTokenManager,
                                   IHttpClientFactory httpClientFactory,
                                   // ReSharper disable once SuggestBaseTypeForParameterInConstructor
                                   ILogger<KeycloakUserGroupWriter> logger)
        : base(bearerTokenManager, httpClientFactory, logger)
    {
    }

    async Task<int> IKeycloakUserGroupWriter.CreateGroupAssignmentsAsync(KeycloakEndpoint keycloakEndpoint,
                                                                         IReadOnlyDictionary<KeycloakUserId, KeycloakGroupId[]>
                                                                             groupAssignmentsToCreate)
    {
        var httpClient = GetHttpClient();
        var assignedGroupMemberships = 0;
        foreach (var uri in from user in groupAssignmentsToCreate
                            from keycloakGroupId in user.Value
                            select new Uri(keycloakEndpoint.BaseUrl, $"{keycloakEndpoint.Realm}/users/{user.Key}/groups/{keycloakGroupId}"))
        {
            var request = new HttpRequestMessage(HttpMethod.Put, uri);
            var addedToGroup = await httpClient.PerformAuthorizedRequestAsync(request,
                                                                              BearerTokenManager,
                                                                              keycloakEndpoint,
                                                                              HandleResponseMessage);
            if (addedToGroup)
                assignedGroupMemberships++;
        }

        return assignedGroupMemberships;

        Task<bool> HandleResponseMessage(HttpResponseMessage? responseMessage) =>
            Task.FromResult(responseMessage is { StatusCode: HttpStatusCode.NoContent });
    }

    async Task<int> IKeycloakUserGroupWriter.RemoveGroupAssignmentsAsync(KeycloakEndpoint keycloakEndpoint,
                                                                         IReadOnlyDictionary<KeycloakUserId, KeycloakGroupId[]>
                                                                             groupAssignmentsToRemove)
    {
        var httpClient = GetHttpClient();
        var removedGroupMemberships = 0;
        foreach (var uri in from user in groupAssignmentsToRemove
                            from keycloakGroupId in user.Value
                            select new Uri(keycloakEndpoint.BaseUrl, $"{keycloakEndpoint.Realm}/users/{user.Key}/groups/{keycloakGroupId}"))
        {
            var request = new HttpRequestMessage(HttpMethod.Delete, uri);
            var removedFromGroup = await httpClient.PerformAuthorizedRequestAsync(request,
                                                                                  BearerTokenManager,
                                                                                  keycloakEndpoint,
                                                                                  HandleResponseMessage);
            if (removedFromGroup)
                removedGroupMemberships++;
        }

        return removedGroupMemberships;

        Task<bool> HandleResponseMessage(HttpResponseMessage? responseMessage) =>
            Task.FromResult(responseMessage is { StatusCode: HttpStatusCode.NoContent });
    }
}