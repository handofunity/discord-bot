using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using KeycloakEndpoint = HoU.GuildBot.Shared.Objects.KeycloakEndpoint;

namespace HoU.GuildBot.DAL.Keycloak;

public class KeycloakAccess : IKeycloakAccess
{
    private readonly IBearerTokenManager<KeycloakAccess> _bearerTokenManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<KeycloakAccess> _logger;

    public KeycloakAccess(IBearerTokenManager<KeycloakAccess> bearerTokenManager,
                          IHttpClientFactory httpClientFactory,
                          ILogger<KeycloakAccess> logger)
    {
        _bearerTokenManager = bearerTokenManager;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    private void LogRequestError(Uri uri,
                                 Exception e)
    {
        var baseExceptionMessage = e.GetBaseException().Message;
        _logger.LogRequestError(uri.GetLeftPart(UriPartial.Authority), uri.PathAndQuery, baseExceptionMessage);
    }

    private Func<Task<HttpResponseMessage?>> InvokeHttpGetRequest(HttpClient httpClient,
                                                                  Uri uri) =>
        async () =>
        {
            try
            {
                return await httpClient.GetAsync(uri);
            }
            catch (HttpRequestException e)
            {
                LogRequestError(uri, e);
                return null;
            }
        };

    private Func<Task<HttpResponseMessage?>> InvokeHttpPostRequest(HttpClient httpClient,
                                                                   Uri uri) =>
        async () =>
        {
            try
            {
                return await httpClient.PostAsync(uri, null);
            }
            catch (HttpRequestException e)
            {
                LogRequestError(uri, e);
                return null;
            }
        };

    private Func<Task<HttpResponseMessage?>> InvokeHttpPostRequest<TPayload>(HttpClient httpClient,
                                                                             Uri uri,
                                                                             TPayload payload) =>
        async () =>
        {
            try
            {
                return await httpClient.PostAsJsonAsync(uri, payload);
            }
            catch (HttpRequestException e)
            {
                LogRequestError(uri, e);
                return null;
            }
        };

    private Func<Task<HttpResponseMessage?>> InvokeHttpPutRequest(HttpClient httpClient,
                                                                  Uri uri) =>
        async () =>
        {
            try
            {
                return await httpClient.PutAsync(uri, null);
            }
            catch (HttpRequestException e)
            {
                LogRequestError(uri, e);
                return null;
            }
        };

    private Func<Task<HttpResponseMessage?>> InvokeHttpPutRequest(HttpClient httpClient,
                                                                  Uri uri,
                                                                  JsonNode json) =>
        async () =>
        {
            try
            {
                return await httpClient.PutAsync(uri, new StringContent(json.ToJsonString(), Encoding.UTF8, "application/json"));
            }
            catch (HttpRequestException e)
            {
                LogRequestError(uri, e);
                return null;
            }
        };

    private Func<Task<HttpResponseMessage?>> InvokeHttpDeleteRequest(HttpClient httpClient,
                                                                     Uri uri) =>
        async () =>
        {
            try
            {
                return await httpClient.DeleteAsync(uri);
            }
            catch (HttpRequestException e)
            {
                LogRequestError(uri, e);
                return null;
            }
        };

    private async Task<JsonNode?> TryGetJsonRootFromResponseAsync(HttpResponseMessage? responseMessage,
                                                                  Uri uri)
    {
        if (responseMessage is null)
            return null;
        if (!responseMessage.IsSuccessStatusCode)
        {
            await _logger.LogRequestErrorAsync(uri.GetLeftPart(UriPartial.Authority), uri.PathAndQuery, responseMessage);
            return null;
        }

        var body = await responseMessage.Content.ReadAsStreamAsync();
        return JsonNode.Parse(body);
    }

    private async Task<KeycloakGroup[]?> GetAllGroupsInRealmAsync(HttpClient httpClient,
                                                                  KeycloakEndpoint endpoint)
    {
        var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/groups?briefRepresentation=false");
        return await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                              endpoint,
                                                              ExecuteHttpCall,
                                                              HandleResponseMessage);

        async Task<HttpResponseMessage?> ExecuteHttpCall()
        {
            try
            {
                return await httpClient.GetAsync(uri);
            }
            catch (HttpRequestException e)
            {
                LogRequestError(uri, e);
                return null;
            }
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
        var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/groups/{groupId}/members?briefRepresentation=true");
        return await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
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

            return (from user in users
                    select user?["id"]?.GetValue<Guid?>()
                    into id
                    where id is not null
                    select (KeycloakUserId)id.Value).ToArray();
        }
    }

    private async Task<DiscordUserId?> GetDiscordIdentityProviderIdAsync(HttpClient httpClient,
                                                                         KeycloakEndpoint endpoint,
                                                                         KeycloakUserId userId)
    {
        var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{userId}/federated-identity");
        return await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                              endpoint,
                                                              InvokeHttpGetRequest(httpClient, uri),
                                                              HandleResponseMessage);

        async Task<DiscordUserId?> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            JsonNode? json;
            if ((json = await TryGetJsonRootFromResponseAsync(responseMessage, uri)) is null)
                return null;

            var identityProviders = json.AsArray();
            if (identityProviders.Count == 0)
                return null;

            foreach (var identityProvider in identityProviders)
            {
                if (identityProvider is null)
                    continue;

                if (identityProvider["identityProvider"]?.GetValue<string?>() != "discord")
                    continue;

                var idpUserId = identityProvider["userId"]?.GetValue<string?>();
                if (idpUserId is not null && ulong.TryParse(idpUserId, out var discordUserId))
                    return (DiscordUserId)discordUserId;
            }

            return null;
        }
    }

    private async Task<KeycloakUserId[]?> GetDisabledUsersForDateAsync(HttpClient httpClient,
                                                                       KeycloakEndpoint endpoint,
                                                                       DateOnly? date)
    {
        var dateFilter = date is null
                             ? ""
                             : $"&q=delete_after:{date:yyyy-MM-dd}";
        var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/?enabled=false{dateFilter}&briefRepresentation=true");

        return await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
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

    private async Task<int?> DeleteUsersByIdAsync(HttpClient httpClient,
                                                  KeycloakEndpoint endpoint,
                                                  IEnumerable<KeycloakUserId> userIds)
    {
        var deletedCount = 0;

        foreach (var userId in userIds)
        {
            var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{userId}");
            var deleted = await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
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

    private async Task<Dictionary<DiscordUserId, KeycloakUserId>> AddUsersAsync(HttpClient httpClient,
                                                                                KeycloakEndpoint endpoint,
                                                                                IEnumerable<UserModel> usersToAdd)
    {
        var result = new Dictionary<DiscordUserId, KeycloakUserId>();
        var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users");

        foreach (var user in usersToAdd)
        {
            var request = new CreateUserRequest(user);
            var newKeycloakUserId = await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                                                   endpoint,
                                                                                   InvokeHttpPostRequest(httpClient, uri, request),
                                                                                   HandleResponseMessage);
            if (newKeycloakUserId is not null)
                result.Add(user.DiscordUserId, newKeycloakUserId.Value);
        }

        return result;

        Task<KeycloakUserId?> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            if (responseMessage is null)
                return Task.FromResult<KeycloakUserId?>(null);

            if (responseMessage.StatusCode == HttpStatusCode.Created)
                // Format: https://DOMAIN/admin/realms/REALM/users/GUID
                return Task.FromResult((KeycloakUserId?)Guid.Parse(responseMessage.Headers.Location!.Segments.Last()));

            _logger.LogRequestError(uri.Host,
                                    uri.PathAndQuery,
                                    $"Creating Keycloak user failed: {responseMessage.StatusCode}");
            return Task.FromResult<KeycloakUserId?>(null);
        }
    }

    private async Task<ShortUserRepresentation[]> GetShortUserRepresentationsAsync(HttpClient httpClient,
                                                                                   KeycloakEndpoint endpoint,
                                                                                   IEnumerable<KeycloakUserId> userIds)
    {
        var result = new List<ShortUserRepresentation>();

        foreach (var userId in userIds)
        {
            var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{userId}");
            var user = await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                                      endpoint,
                                                                      InvokeHttpGetRequest(httpClient, uri),
                                                                      HandleResponseMessage);
            if (user is not null)
                result.Add(user);

            async Task<ShortUserRepresentation?> HandleResponseMessage(HttpResponseMessage? responseMessage)
            {
                if (responseMessage is null)
                    return null;

                if (responseMessage.StatusCode != HttpStatusCode.OK)
                {
                    _logger.LogRequestError(uri.Host,
                                            uri.PathAndQuery,
                                            $"Getting Keycloak user failed: {responseMessage.StatusCode}");
                    return null;
                }

                JsonNode? json;
                return (json = await TryGetJsonRootFromResponseAsync(responseMessage, uri)) is null
                           ? null
                           : ShortUserRepresentation.FromJsonAsync(userId, json);
            }
        }

        return result.ToArray();
    }

    private async Task<int> UpdateUsersAsync(HttpClient httpClient,
                                             KeycloakEndpoint endpoint,
                                             IEnumerable<ShortUserRepresentation> users)
    {
        var updatedUsers = 0;

        foreach (var user in users)
        {
            var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{user.UserId}");
            var json = user.ToJson();
            var updated = await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                                         endpoint,
                                                                         InvokeHttpPutRequest(httpClient, uri, json),
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
                                              IEnumerable<KeycloakUserId> userIds)
    {
        var users = await GetShortUserRepresentationsAsync(httpClient,
                                                           endpoint,
                                                           userIds);
        var inSixMonths = DateTime.UtcNow.AddMonths(6).ToString("yyyy-MM-dd");

        foreach (var user in users)
        {
            user.Enabled = false;
            user.Attributes[ShortUserRepresentation.KnownAttributes.DeleteAfter] = inSixMonths;
        }

        return await UpdateUsersAsync(httpClient, endpoint, users);
    }

    private async Task<int> EnableUsersAsync(HttpClient httpClient,
                                             KeycloakEndpoint endpoint,
                                             IEnumerable<KeycloakUserId> userIds)
    {
        var users = await GetShortUserRepresentationsAsync(httpClient,
                                                           endpoint,
                                                           userIds);

        foreach (var user in users)
        {
            user.Enabled = true;
            if (user.Attributes.ContainsKey(ShortUserRepresentation.KnownAttributes.DeleteAfter))
                user.Attributes.Remove(ShortUserRepresentation.KnownAttributes.DeleteAfter);
        }

        return await UpdateUsersAsync(httpClient, endpoint, users);
    }

    private async Task<int> LogoutUsersAsync(HttpClient httpClient,
                                             KeycloakEndpoint endpoint,
                                             IEnumerable<KeycloakUserId> usersToDisable)
    {
        var loggedOutUsers = 0;

        foreach (var userId in usersToDisable)
        {
            var uri = new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{userId}/logout");
            var loggedOut = await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                                           endpoint,
                                                                           InvokeHttpPostRequest(httpClient, uri),
                                                                           HandleResponseMessage);
            if (loggedOut)
                loggedOutUsers++;
        }

        return loggedOutUsers;

        Task<bool> HandleResponseMessage(HttpResponseMessage? responseMessage) =>
            Task.FromResult(responseMessage is { StatusCode: HttpStatusCode.NoContent });
    }

    private async Task<int> AddToGroupsAsync(HttpClient httpClient,
                                             KeycloakEndpoint endpoint,
                                             Dictionary<KeycloakUserId, KeycloakGroupId[]> userGroupsToAdd)
    {
        var assignedGroupMemberships = 0;
        foreach (var uri in from user in userGroupsToAdd
                            from keycloakGroupId in user.Value
                            select new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{user.Key}/groups/{keycloakGroupId}"))
        {
            var addedToGroup = await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                                              endpoint,
                                                                              InvokeHttpPutRequest(httpClient, uri),
                                                                              HandleResponseMessage);
            if (addedToGroup)
                assignedGroupMemberships++;
        }

        return assignedGroupMemberships;

        Task<bool> HandleResponseMessage(HttpResponseMessage? responseMessage) =>
            Task.FromResult(responseMessage is { StatusCode: HttpStatusCode.NoContent });
    }

    private async Task<int> RemoveGroupsAsync(HttpClient httpClient,
                                              KeycloakEndpoint endpoint,
                                              Dictionary<KeycloakUserId, KeycloakGroupId[]> userGroupsToRemove)
    {
        var removedGroupMemberships = 0;

        foreach (var uri in from user in userGroupsToRemove
                            from keycloakGroupId in user.Value
                            select new Uri(endpoint.BaseUrl, $"{endpoint.Realm}/users/{user.Key}/groups/{keycloakGroupId}"))
        {
            var removedFromGroup = await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                                                  endpoint,
                                                                                  InvokeHttpDeleteRequest(httpClient, uri),
                                                                                  HandleResponseMessage);
            if (removedFromGroup)
                removedGroupMemberships++;
        }

        return removedGroupMemberships;

        Task<bool> HandleResponseMessage(HttpResponseMessage? responseMessage) =>
            Task.FromResult(responseMessage is { StatusCode: HttpStatusCode.NoContent });
    }

    async Task<ConfiguredKeycloakGroups?> IKeycloakAccess.GetConfiguredGroupsAsync(KeycloakEndpoint endpoint)
    {
        var httpClient = _httpClientFactory.CreateClient("keycloak");

        var allRoles = await GetAllGroupsInRealmAsync(httpClient, endpoint);
        if (allRoles is null)
            return null;

        return new ConfiguredKeycloakGroups(allRoles.Where(m => m.DiscordRoleId is not null && m.IsFallbackGroup == false)
                                                    .ToDictionary(m => m.DiscordRoleId!.Value, m => m.KeycloakGroupId),
                                            allRoles.Single(m => m.IsFallbackGroup).KeycloakGroupId);
    }

    async Task<KeycloakUserId[]?> IKeycloakAccess.GetGroupMembersAsync(KeycloakEndpoint endpoint,
                                                                       KeycloakGroupId groupId)
    {
        var httpClient = _httpClientFactory.CreateClient("keycloak");

        return await GetGroupMemberIdsAsync(httpClient,
                                            endpoint,
                                            groupId);
    }

    async Task<DiscordUserId?> IKeycloakAccess.GetDiscordUserIdAsync(KeycloakEndpoint endpoint,
                                                                     KeycloakUserId userId)
    {
        var httpClient = _httpClientFactory.CreateClient("keycloak");

        return await GetDiscordIdentityProviderIdAsync(httpClient,
                                                       endpoint,
                                                       userId);
    }

    async Task<SyncAllUsersResponse?> IKeycloakAccess.SendDiffAsync(KeycloakEndpoint endpoint,
                                                                    KeycloakDiscordDiff diff)
    {
        var httpClient = _httpClientFactory.CreateClient("keycloak");

        if (diff.UsersToAdd.Any())
        {
            var newUsers = await AddUsersAsync(httpClient, endpoint, diff.UsersToAdd.Select(m => m.DiscordUser));
            var rolesToAddToNewUsers = newUsers.Join(diff.UsersToAdd,
                                                     kvp => kvp.Key,
                                                     tuple => tuple.DiscordUser.DiscordUserId,
                                                     (kvp,
                                                      tuple) => new
                                                     {
                                                         KeycloakUserId = kvp.Value,
                                                         tuple.KeycloakGroupIds
                                                     });
            foreach (var newUser in rolesToAddToNewUsers)
                diff.GroupsToAdd.Add(newUser.KeycloakUserId, newUser.KeycloakGroupIds);
        }

        if (diff.UsersToDisable.Any())
        {
            var disabledUsers = await DisableUsersAsync(httpClient, endpoint, diff.UsersToDisable);
            var loggedOutUsers = await LogoutUsersAsync(httpClient, endpoint, diff.UsersToDisable);
        }

        if (diff.UsersToEnable.Any())
        {
            var enabledUsers = await EnableUsersAsync(httpClient, endpoint, diff.UsersToEnable);
        }

        if (diff.GroupsToAdd.Any())
        {
            var assignedGroupMemberships = await AddToGroupsAsync(httpClient, endpoint, diff.GroupsToAdd);
        }

        if (diff.GroupsToRemove.Any())
        {
            var removedGroupMemberships = await RemoveGroupsAsync(httpClient, endpoint, diff.GroupsToRemove);
        }

        return new SyncAllUsersResponse();
    }

    async Task<KeycloakUserId[]?> IKeycloakAccess.GetUsersFlaggedForDeletionAsync(KeycloakEndpoint endpoint,
                                                                                  DateOnly? date)
    {
        var httpClient = _httpClientFactory.CreateClient("keycloak");

        return await GetDisabledUsersForDateAsync(httpClient, endpoint, date);
    }

    async Task<int?> IKeycloakAccess.DeleteUsersAsync(KeycloakEndpoint endpoint,
                                                      KeycloakUserId[] userIds)
    {
        var httpClient = _httpClientFactory.CreateClient("keycloak");

        return await DeleteUsersByIdAsync(httpClient, endpoint, userIds);
    }

    private record KeycloakGroup(KeycloakGroupId KeycloakGroupId,
                                 DiscordRoleId? DiscordRoleId,
                                 bool IsFallbackGroup);

    [UsedImplicitly(ImplicitUseTargetFlags.Members)]
    private class CreateUserRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; }

        [JsonPropertyName("enabled")]
        public bool Enabled { get; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; }

        [JsonPropertyName("federatedIdentities")]
        public FederatedIdentity[] FederatedIdentities { get; }

        public CreateUserRequest(UserModel userModel)
        {
            Username = $"{userModel.Username.ToLower()}#{userModel.Discriminator:D4}";
            Enabled = true;
            FirstName = userModel.Username;
            FederatedIdentities = new[]
            {
                new FederatedIdentity(userModel.DiscordUserId, Username)
            };
        }

        [UsedImplicitly(ImplicitUseTargetFlags.Members)]
        public class FederatedIdentity
        {
            private const string DiscordIdentityProviderName = "discord";

            [JsonPropertyName("identityProvider")]
            public string IdentityProvider { get; }

            [JsonPropertyName("userId")]
            public string UserId { get; }

            [JsonPropertyName("userName")]
            public string Username { get; }

            public FederatedIdentity(DiscordUserId userId,
                                     string username)
            {
                IdentityProvider = DiscordIdentityProviderName;
                UserId = userId.ToString();
                Username = username;
            }
        }
    }

    private class ShortUserRepresentation
    {
        public KeycloakUserId UserId { get; }

        public bool Enabled { get; set; }

        public Dictionary<string, string> Attributes { get; }

        private ShortUserRepresentation(KeycloakUserId userId,
                                        bool enabled)
        {
            UserId = userId;
            Enabled = enabled;
            Attributes = new Dictionary<string, string>();
        }

        internal static ShortUserRepresentation FromJsonAsync(KeycloakUserId userId,
                                                              JsonNode json)
        {
            var result = new ShortUserRepresentation(userId,
                                                     json["enabled"]?.GetValue<bool>() ?? false);

            var attributes = json["attributes"]?.AsObject();
            if (attributes is null)
                return result;

            foreach (var property in attributes)
            {
                var propertyValue = property.Value?.AsArray().FirstOrDefault()?.GetValue<string?>();
                if (propertyValue is not null)
                    result.Attributes[property.Key] = propertyValue;
            }

            return result;
        }

        internal JsonNode ToJson()
        {
            var attributes = new JsonObject();
            foreach (var attribute in Attributes)
            {
                attributes[attribute.Key] = new JsonArray(JsonValue.Create(attribute.Value));
            }

            return new JsonObject
            {
                ["enabled"] = JsonValue.Create(Enabled),
                ["attributes"] = attributes
            };
        }

        internal class KnownAttributes
        {
            internal const string DeleteAfter = "delete_after";
        }
    }
}