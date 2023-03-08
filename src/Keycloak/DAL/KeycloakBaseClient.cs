namespace HoU.GuildBot.Keycloak.DAL;

internal abstract class KeycloakBaseClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    protected IBearerTokenManager<KeycloakBaseClient> BearerTokenManager { get; }
    protected ILogger Logger { get; }

    protected KeycloakBaseClient(IBearerTokenManager<KeycloakBaseClient> bearerTokenManager,
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

    protected Func<Task<HttpResponseMessage?>> InvokeHttpGetRequest(HttpClient httpClient,
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

    protected Func<Task<HttpResponseMessage?>> InvokeHttpPostRequest<TPayload>(HttpClient httpClient,
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

    protected Func<Task<HttpResponseMessage?>> InvokeHttpPostRequest(HttpClient httpClient,
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

    protected Func<Task<HttpResponseMessage?>> InvokeHttpPutRequest(HttpClient httpClient,
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

    protected Func<Task<HttpResponseMessage?>> InvokeHttpPutRequest(HttpClient httpClient,
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

    protected Func<Task<HttpResponseMessage?>> InvokeHttpDeleteRequest(HttpClient httpClient,
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