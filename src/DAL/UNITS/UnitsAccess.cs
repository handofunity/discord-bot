using System.Reactive;
using Newtonsoft.Json;
using UnitsEndpoint = HoU.GuildBot.Shared.Objects.UnitsEndpoint;

namespace HoU.GuildBot.DAL.UNITS;

public class UnitsAccess : IUnitsAccess
{
    private readonly IBearerTokenManager<UnitsAccess> _bearerTokenManager;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<UnitsAccess> _logger;

    public UnitsAccess(IBearerTokenManager<UnitsAccess> bearerTokenManager,
                       IHttpClientFactory httpClientFactory,
                       ILogger<UnitsAccess> logger)
    {
        _bearerTokenManager = bearerTokenManager ?? throw new ArgumentNullException(nameof(bearerTokenManager));
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    async Task IUnitsAccess.SendCreatedVoiceChannelsAsync(UnitsEndpoint unitsEndpoint,
                                                          SyncCreatedVoiceChannelsRequest createdVoiceChannelsRequest)
    {
        const string requestPath = "/bot-api/discordsync/created-voice-channels";
        var serialized = JsonConvert.SerializeObject(createdVoiceChannelsRequest);
        var requestContent = new StringContent(serialized, Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient("units");
        await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                  unitsEndpoint,
                                                  ExecuteHttpCall,
                                                  HandleResponseMessage);

        async Task<HttpResponseMessage?> ExecuteHttpCall()
        {
            try
            {
                return await httpClient.PostAsync(requestPath, requestContent);
            }
            catch (HttpRequestException e)
            {
                var baseExceptionMessage = e.GetBaseException().Message;
                _logger.LogRequestError(unitsEndpoint.BaseAddress.ToString(), requestPath, baseExceptionMessage);
                return null;
            }
        }

        async Task<Unit> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            if (responseMessage == null)
                return Unit.Default;
            if (!responseMessage.IsSuccessStatusCode)
                await _logger.LogRequestErrorAsync(unitsEndpoint.BaseAddress.ToString(), requestPath, responseMessage);
            return Unit.Default;
        }
    }

    async Task IUnitsAccess.SendCurrentAttendeesAsync(UnitsEndpoint unitsEndpoint,
                                                      SyncCurrentAttendeesRequest currentAttendeesRequest)
    {
        const string requestPath = "/bot-api/discordsync/current-attendees";
        var serialized = JsonConvert.SerializeObject(currentAttendeesRequest);
        var requestContent = new StringContent(serialized, Encoding.UTF8, "application/json");

        var httpClient = _httpClientFactory.CreateClient("units");
        await httpClient.PerformAuthorizedRequestAsync(_bearerTokenManager,
                                                  unitsEndpoint,
                                                  ExecuteHttpCall,
                                                  HandleResponseMessage);

        async Task<HttpResponseMessage?> ExecuteHttpCall()
        {
            try
            {
                return await httpClient.PostAsync(requestPath, requestContent);
            }
            catch (HttpRequestException e)
            {
                var baseExceptionMessage = e.GetBaseException().Message;
                _logger.LogRequestError(unitsEndpoint.BaseAddress.ToString(), requestPath, baseExceptionMessage);
                return null;
            }
        }

        async Task<Unit> HandleResponseMessage(HttpResponseMessage? responseMessage)
        {
            if (responseMessage == null)
                return Unit.Default;
            if (!responseMessage.IsSuccessStatusCode)
                await _logger.LogRequestErrorAsync(unitsEndpoint.ToString(), requestPath, responseMessage);
            return Unit.Default;
        }
    }
}