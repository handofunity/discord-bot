namespace HoU.GuildBot.Shared.DAL;

public interface IBearerTokenManager
{
    Task GetAndSetBearerTokenAsync(HttpRequestMessage httpRequestMessage,
                                   Uri targetUrl,
                                   AuthorizationEndpoint authorizationEndpoint,
                                   bool forceRefresh);

    Task<string?> GetBearerTokenAsync(Uri targetUrl,
                                      AuthorizationEndpoint authorizationEndpoint,
                                      bool forceRefresh);
}