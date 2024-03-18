using System.Threading;

namespace HoU.GuildBot.DAL.UNITS;

public abstract class UnitsBaseClient
{
    public AuthorizationEndpoint? AuthorizationEndpoint { private get; set; }

    public IBearerTokenManager? BearerTokenManager { private get; set; }
    
    protected static void UpdateJsonSerializerSettings(JsonSerializerOptions settings)
    {
        settings.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    }
    
    protected async Task PrepareRequestAsync(HttpClient client,
                                             HttpRequestMessage request,
                                             string url,
                                             CancellationToken cancellationToken)
    {
        if (AuthorizationEndpoint is null)
            throw new InvalidOperationException($"{nameof(AuthorizationEndpoint)} endpoint not set");
        if (BearerTokenManager is null)
            throw new InvalidOperationException($"{nameof(BearerTokenManager)} endpoint not set");

        var baseUrl = new Uri(url).GetLeftPart(UriPartial.Authority);
        await BearerTokenManager.GetAndSetBearerTokenAsync(request,
                                                      new Uri(baseUrl),
                                                      AuthorizationEndpoint,
                                                      true);
    }

    protected static Task PrepareRequestAsync(HttpClient client,
                                              HttpRequestMessage request,
                                              StringBuilder urlBuilder,
                                              CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    protected static Task ProcessResponseAsync(HttpClient client,
                                               HttpResponseMessage response,
                                               CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}