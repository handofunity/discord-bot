namespace HoU.GuildBot.Keycloak.DAL;

internal class KeycloakGroupReader : KeycloakBaseClient, IKeycloakGroupReader
{
    public KeycloakGroupReader(IBearerTokenManager bearerTokenManager,
                               IHttpClientFactory httpClientFactory,
                               // ReSharper disable once SuggestBaseTypeForParameterInConstructor
                               ILogger<KeycloakGroupReader> logger)
        : base(bearerTokenManager, httpClientFactory, logger)
    {
    }

    private async Task<KeycloakGroup[]?> GetAllGroupsInRealmAsync(HttpClient httpClient,
                                                                  KeycloakEndpoint endpoint)
    {
        var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/groups?briefRepresentation=false");
        var request = new HttpRequestMessage(HttpMethod.Get, uri);

        try
        {
            return await httpClient.PerformAuthorizedRequestAsync(request,
                                                                  BearerTokenManager,
                                                                  endpoint,
                                                                  HandleResponseMessage);
        }
        catch (HttpRequestException e)
        {
            LogRequestError(uri, e);
            return null;
        }

        async Task<KeycloakGroup[]?> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            JsonNode? json;
            if ((json = await TryGetJsonRootFromResponseAsync(responseMessage, uri)) is null)
                return null;

            var groups = json.AsArray();
            var result = new List<KeycloakGroup>();
            foreach (var group in groups)
            {
                var id = group?["id"]?.GetValue<Guid?>();
                if (id is null)
                    continue;

                // Either the attribute "discord_role_id" or "discord_fallback_group" must be present.
                // If neither is present, it's not a group relevant for synchronization.
                var attributes = group?["attributes"];
                if (attributes is null)
                    continue;

                var discordRoleId = attributes["discord_role_id"]?.AsArray().FirstOrDefault()?.GetValue<string?>();
                if (discordRoleId is not null && ulong.TryParse(discordRoleId, out var discordRoleIdValue))
                {
                    result.Add(new KeycloakGroup((KeycloakGroupId)id.Value, (DiscordRoleId)discordRoleIdValue, false));
                    continue;
                }

                var discordFallbackGroup = attributes["discord_fallback_group"]?.AsArray().FirstOrDefault()?.GetValue<string?>();
                if (discordFallbackGroup is not null
                 && bool.TryParse(discordFallbackGroup, out var discordFallbackGroupValue)
                 && discordFallbackGroupValue)
                {
                    result.Add(new KeycloakGroup((KeycloakGroupId)id.Value, null, true));
                }
            }

            return result.ToArray();
        }
    }

    private async Task<KeycloakUserId[]?> GetGroupMemberIdsAsync(HttpClient httpClient,
                                                                 KeycloakEndpoint endpoint,
                                                                 KeycloakGroupId groupId)
    {
        var result = new List<KeycloakUserId>();

        var offset = 0;
        do
        {
            var batch = await GetGroupMemberIdsByOffsetAsync(offset);
            if (batch is null || batch.Length == 0)
                break;
            
            result.AddRange(batch);

            offset += 100;
        } while (result.Count % 100 == 0);
        
        return result.ToArray();

        async Task<KeycloakUserId[]?> GetGroupMemberIdsByOffsetAsync(int innerOffset)
        {
            var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/groups/{groupId}/"
                                              + $"members?briefRepresentation=true&first={innerOffset}&max=100");
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            return await httpClient.PerformAuthorizedRequestAsync(request,
                                                                  BearerTokenManager,
                                                                  endpoint,
                                                                  HandleResponseMessage);

            async Task<KeycloakUserId[]?> HandleResponseMessage(HttpResponseMessage? responseMessage)
            {
                JsonNode? json;
                if ((json = await TryGetJsonRootFromResponseAsync(responseMessage, uri)) is null)
                    return null;

                var users = json.AsArray();
                if (users.Count == 0)
                    return Array.Empty<KeycloakUserId>();

                return (from user in users
                        select user?["id"]?.GetValue<Guid?>()
                        into id
                        where id is not null
                        select (KeycloakUserId)id.Value).ToArray();
            }
        }
    }
    
    async Task<ConfiguredKeycloakGroups?> IKeycloakGroupReader.GetConfiguredGroupsAsync(KeycloakEndpoint endpoint)
    {
        var httpClient = GetHttpClient();
        var allRoles = await GetAllGroupsInRealmAsync(httpClient, endpoint);
        if (allRoles is null)
            return null;

        return new ConfiguredKeycloakGroups(allRoles.Where(m => m.DiscordRoleId is not null && m.IsFallbackGroup == false)
                                                    .ToDictionary(m => m.DiscordRoleId!.Value, m => m.KeycloakGroupId),
                                            allRoles.Single(m => m.IsFallbackGroup).KeycloakGroupId);
    }

    async Task<KeycloakUserId[]?> IKeycloakGroupReader.GetGroupMembersAsync(KeycloakEndpoint endpoint,
                                                                            KeycloakGroupId groupId)
    {
        var httpClient = GetHttpClient();
        return await GetGroupMemberIdsAsync(httpClient,
                                            endpoint,
                                            groupId);
    }
}