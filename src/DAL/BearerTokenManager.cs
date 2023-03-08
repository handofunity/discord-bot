namespace HoU.GuildBot.DAL;

public class BearerTokenManager<TClient> : IBearerTokenManager<TClient>
    where TClient : class
{
    private readonly ILogger<BearerTokenManager<TClient>> _logger;
    private readonly Dictionary<Uri, string> _lastBearerTokens;

    public BearerTokenManager(ILogger<BearerTokenManager<TClient>> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _lastBearerTokens = new Dictionary<Uri, string>();
    }

    public async Task<bool> GetAndSetBearerToken(HttpClient httpClient,
                                                 AuthorizationEndpoint authorizationEndpoint,
                                                 bool forceRefresh)
    {
        var bearerToken = await GetBearerTokenAsync(httpClient, authorizationEndpoint, forceRefresh);
        if (bearerToken == null)
            return false;

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        return true;
    }

    public async Task<string?> GetBearerTokenAsync(HttpClient httpClient,
                                                   AuthorizationEndpoint authorizationEndpoint,
                                                   bool forceRefresh)
    {
        if (!forceRefresh && _lastBearerTokens.TryGetValue(authorizationEndpoint.AccessTokenUrl, out var lastToken))
        {
            // Check if token expires within the next 10 seconds.
            var token = new JwtSecurityToken(lastToken);
            if (token.ValidTo > DateTime.UtcNow.AddSeconds(10))
                return lastToken;
        }

        HttpResponseMessage response;
        try
        {
            var values = new Dictionary<string, string>
            {
                {"grant_type", "client_credentials"},
                {"client_id", authorizationEndpoint.ClientId},
                {"client_secret", authorizationEndpoint.ClientSecret},
            };
            var request = new HttpRequestMessage(HttpMethod.Post, authorizationEndpoint.AccessTokenUrl)
            {
                Content = new FormUrlEncodedContent(values)
            };
            request.Headers.Add("cache-control", "no-cache");
            response = await httpClient.SendAsync(request);
        }
        catch (HttpRequestException e)
        {
            var baseExceptionMessage = e.GetBaseException().Message;
            _logger.LogRequestError(authorizationEndpoint.AccessTokenBaseAddress, authorizationEndpoint.AccessTokenRoute, baseExceptionMessage);
            return null;
        }

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStreamAsync();
            var jsonContent = await JsonDocument.ParseAsync(content);
            var tokenResponse = OAuthTokenResponse.Success(jsonContent);
            if (tokenResponse.AccessToken is null)
            {
                await _logger.LogRequestErrorAsync(authorizationEndpoint.AccessTokenBaseAddress, authorizationEndpoint.AccessTokenRoute, response);
                return null;
            }
            
            _lastBearerTokens[authorizationEndpoint.AccessTokenUrl] = tokenResponse.AccessToken;
            return tokenResponse.AccessToken;
        }

        await _logger.LogRequestErrorAsync(authorizationEndpoint.AccessTokenBaseAddress, authorizationEndpoint.AccessTokenRoute, response);
        return null;
    }
}