using UnitsEndpoint = HoU.GuildBot.Shared.Objects.UnitsEndpoint;

namespace HoU.GuildBot.DAL.UNITS;

public class UnitsSignalRClient : IUnitsSignalRClient
{
    private readonly IBearerTokenManager _bearerTokenManager;
    private readonly IUnitsBotClient _botClient;
    private readonly ILogger<UnitsSignalRClient> _logger;
    private const string BotHubRoute = "/bot-hub";

    private readonly Dictionary<Uri, HubConnection> _hubConnections;
    private readonly Dictionary<Uri, bool> _requiresTokenRefresh;

    public UnitsSignalRClient(IHttpClientFactory httpClientFactory,
                              IBearerTokenManager bearerTokenManager,
                              IUnitsBotClient botClient,
                              ILogger<UnitsSignalRClient> logger)
    {
        _bearerTokenManager = bearerTokenManager ?? throw new ArgumentNullException(nameof(bearerTokenManager));
        _botClient = botClient ?? throw new ArgumentNullException(nameof(botClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _hubConnections = new Dictionary<Uri, HubConnection>();
        _requiresTokenRefresh = new Dictionary<Uri, bool>();
    }

    private void RegisterHandlers(HubConnection connection,
                                  Uri baseAddress)
    {
        var methods = typeof(IUnitsBotClient)
                     .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                     .Where(m => m.Name.StartsWith("Receive")
                              && m.ReturnType == typeof(Task))
                     .ToArray();
        foreach (var method in methods)
        {
            var allParameters = method.GetParameters();
            var firstParameter = allParameters.FirstOrDefault();
            var withBaseAddress = firstParameter != null
                               && firstParameter.Name == "baseAddress"
                               && firstParameter.ParameterType == typeof(string);
            if (withBaseAddress)
            {
                var actualParameterInfo = allParameters.Skip(1).ToArray();
                var actualParameters = string.Join(", ", actualParameterInfo.Select(m => $"{m.ParameterType} {m.Name}"));
                _logger.LogDebug("Registration of SignalR handler '{MethodName}' with base address parameter and these actual parameters: {ActualParameters}",
                                 method.Name,
                                 actualParameters);
                connection.On(method.Name,
                              actualParameterInfo.Select(m => m.ParameterType).ToArray(),
                              async parameters =>
                              {
                                  if (method.Invoke(_botClient, new object[] { baseAddress }.Concat(parameters).ToArray()) is Task response)
                                      await response;
                              });
            }
            else
            {
                var actualParameters = string.Join(", ", allParameters.Select(m => $"{m.ParameterType} {m.Name}"));
                _logger.LogDebug("Registration of SignalR handler '{MethodName}' with these parameters: {ActualParameters}",
                                 method.Name,
                                 actualParameters);
                connection.On(method.Name,
                              allParameters.Select(m => m.ParameterType).ToArray(),
                              async parameters =>
                              {
                                  if (method.Invoke(_botClient, parameters) is Task response)
                                      await response;
                              });
            }
        }
    }

    async Task IUnitsSignalRClient.ConnectAsync(UnitsEndpoint unitsEndpoint)
    {
        if (_hubConnections.ContainsKey(unitsEndpoint.BaseAddress))
            return;

        var hubRoute = unitsEndpoint.BaseAddress + BotHubRoute;
        var connection = new HubConnectionBuilder()
                        .WithUrl(hubRoute,
                                 options =>
                                 {
#if DEBUG
                                     options.WebSocketConfiguration = conf =>
                                     {
                                         conf.RemoteCertificateValidationCallback = (sender,
                                                                                     certificate,
                                                                                     chain,
                                                                                     errors) => true;
                                     };
                                     options.HttpMessageHandlerFactory = handler =>
                                     {
                                         if (handler is HttpClientHandler clientHandler)
                                         {
                                             clientHandler.ServerCertificateCustomValidationCallback = (message,
                                                 certificate2,
                                                 arg3,
                                                 arg4) => true;
                                         }

                                         return handler;
                                     };
#endif
                                     options.AccessTokenProvider = async () =>
                                     {
                                         var token =
                                             await _bearerTokenManager.GetBearerTokenAsync(unitsEndpoint.BaseAddress,
                                                                                           unitsEndpoint.KeycloakEndpoint,
                                                                                           _requiresTokenRefresh
                                                                                               [unitsEndpoint.BaseAddress]);
                                         _requiresTokenRefresh[unitsEndpoint.BaseAddress] = false;
                                         return token;
                                     };
                                 })
                        .ConfigureLogging(logging =>
                         {
                             logging.SetMinimumLevel(LogLevel.Trace);
                             logging.AddSerilog();
                         })
                        .Build();
        connection.Closed += async (error) =>
        {
            _logger.LogWarning("Connection to UNITS at {BaseUrl} closed. Trying to reconnect ...", unitsEndpoint.BaseAddress);
            do
            {
                try
                {
                    await Task.Delay(new Random().Next(0, 5) * 10_000);
                    if (connection.State != HubConnectionState.Connecting)
                        await connection.StartAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to re-connect to UNITS at {BaseUrl}", unitsEndpoint.BaseAddress);
                    if (e is HttpRequestException httpRequestException && httpRequestException.Message.Contains("401"))
                    {
                        _requiresTokenRefresh[unitsEndpoint.BaseAddress] = true;
                    }
                }
            } while (connection.State != HubConnectionState.Connected);
        };

        RegisterHandlers(connection,
                         unitsEndpoint.BaseAddress);

        _requiresTokenRefresh.Add(unitsEndpoint.BaseAddress, false);
        _hubConnections.Add(unitsEndpoint.BaseAddress, connection);

        _logger.LogInformation("Connecting to UNITS at {BaseUrl} ...", unitsEndpoint.BaseAddress);
        do
        {
            try
            {
                await connection.StartAsync();
                    
                if (connection.State == HubConnectionState.Connected)
                    _logger.LogInformation("Connected to UNITS at {BaseUrl}", unitsEndpoint.BaseAddress);
            }
            catch (Exception e)
            {
                var delaySeconds = new Random().Next(5, 20);
                _logger.LogError(e, "Failed to initially connect to UNITS at {BaseUrl}. Trying again in {Seconds} seconds ...",
                                 unitsEndpoint.BaseAddress,
                                 delaySeconds);
                await Task.Delay(delaySeconds * 1000);
            }
        } while (connection.State != HubConnectionState.Connected);
    }
}