namespace HoU.GuildBot.Keycloak.DAL;

internal abstract class KeycloakBaseClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    protected IBearerTokenManager BearerTokenManager { get; }
    protected ILogger Logger { get; }

    protected KeycloakBaseClient(IBearerTokenManager bearerTokenManager,
                                 IHttpClientFactory httpClientFactory,
                                 ILogger logger)
    {
        BearerTokenManager = bearerTokenManager;
        _httpClientFactory = httpClientFactory;
        Logger = logger;
    }

    protected void LogRequestError(Uri uri,
                                   Exception e)
    {
        var baseExceptionMessage = e.GetBaseException().Message;
        Logger.LogRequestError(uri.GetLeftPart(UriPartial.Authority), uri.PathAndQuery, baseExceptionMessage);
    }

    protected HttpClient GetHttpClient() => _httpClientFactory.CreateClient("keycloak");

    protected async Task<JsonNode?> TryGetJsonRootFromResponseAsync(HttpResponseMessage? responseMessage,
                                                                    Uri uri)
    {
        if (responseMessage is null)
            return null;
        if (!responseMessage.IsSuccessStatusCode)
        {
            await Logger.LogRequestErrorAsync(uri.GetLeftPart(UriPartial.Authority), uri.PathAndQuery, responseMessage);
            return null;
        }

        var body = await responseMessage.Content.ReadAsStreamAsync();
        return JsonNode.Parse(body);
    }
}