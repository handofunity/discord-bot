namespace HoU.GuildBot.Shared.DAL;

public interface IBearerTokenManager<TClient>
    where TClient : class
{
    Task<bool> GetAndSetBearerToken(HttpClient httpClient,
                                    AuthorizationEndpoint authorizationEndpoint,
                                    bool forceRefresh);

    Task<string?> GetBearerTokenAsync(HttpClient httpClient,
                                      AuthorizationEndpoint authorizationEndpoint,
                                      bool forceRefresh);
}