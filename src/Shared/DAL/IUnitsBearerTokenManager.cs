namespace HoU.GuildBot.Shared.DAL;

public interface IUnitsBearerTokenManager
{
    Task<bool> GetAndSetBearerToken(HttpClient httpClient,
                                    string baseAddress,
                                    string secret,
                                    bool refresh);

    Task<string?> GetBearerTokenAsync(HttpClient httpClient,
                                      string baseAddress,
                                      string secret,
                                      bool forceRefresh);
}