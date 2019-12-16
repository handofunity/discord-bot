namespace HoU.GuildBot.DAL
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using Shared.DAL;
    using Shared.Objects;

    public class UnitsAccess : IUnitsAccess
    {
        private const string TokenRoute = "/bot-api/auth/token";
        private const string RoleNamesRoute = "/bot-api/discordsync/valid-role-names";
        private const string PushAllUsersRoute = "/bot-api/discordsync/all-users";

        private readonly AppSettings _appSettings;
        private readonly ILogger<UnitsAccess> _logger;

        private string _lastBearerToken;

        public UnitsAccess(AppSettings appSettings,
                           ILogger<UnitsAccess> logger)
        {
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task UseHttpClient(Func<HttpClient, Task<HttpResponseMessage>> invokeHttpRequest,
                                         Func<HttpResponseMessage, Task> handleResult)
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
#if DEBUG
                httpClientHandler.ServerCertificateCustomValidationCallback = (message,
                                                                               certificate2,
                                                                               arg3,
                                                                               arg4) => true;
#endif
                using (var httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri(_appSettings.UnitsBaseAddress) })
                {
                    httpClient.DefaultRequestHeaders.Accept.Clear();
                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    var tokenSet = await GetAndSetBearerToken(httpClient, false);
                    if (!tokenSet)
                    {
                        // If the Bearer token is not set, the HTTP request will fail anyway.
                        // Instead of invoking it, just return.
                        return;
                    }

                    var response = await invokeHttpRequest(httpClient);

                    if (response == null || response.IsSuccessStatusCode)
                    {
                        // If the result is null, or a success, invoke the result handler.
                        await handleResult(response);
                        return;
                    }

                    if (response.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        // If the first response status code is Unauthorized (401), the token might either be expired, or invalid.
                        var isExpired = response.Headers.TryGetValues("Token-Expired", out var expiredTokenValues) && expiredTokenValues.Any(m => m?.ToLowerInvariant() == "true");
                        if (isExpired)
                        {
                            // If the token is expired, refresh the token.
                            tokenSet = await GetAndSetBearerToken(httpClient, true);
                            if (!tokenSet)
                            {
                                // If the Bearer token is not set, the HTTP request will fail anyway.
                                // Instead of invoking it, just return.
                                return;
                            }

                            // If the token is set, perform the request again.
                            response = await invokeHttpRequest(httpClient);
                        }
                    }

                    // If the status code is no success status code, and not Unauthorized (401), or was 401 and was invoked again with a refreshed token, handle the result here.
                    await handleResult(response);
                }
            }
        }

        private async Task<bool> GetAndSetBearerToken(HttpClient httpClient,
                                                      bool refresh)
        {
            if (!refresh && _lastBearerToken != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _lastBearerToken);
                return true;
            }

            _lastBearerToken = null;

            HttpResponseMessage response;
            try
            {
                var request = new BotAuthenticationRequest
                {
                    ClientId = typeof(UnitsAccess).FullName,
                    ClientSecret = _appSettings.UnitsAccessSecret
                };
                var serialized = JsonConvert.SerializeObject(request);
                response = await httpClient.PostAsync(TokenRoute, new StringContent(serialized, Encoding.UTF8, "application/json"));
            }
            catch (HttpRequestException e)
            {
                var baseExceptionMessage = e.GetBaseException().Message;
                LogRequestError(TokenRoute, baseExceptionMessage);
                return false;
            }

            if (response.IsSuccessStatusCode)
            {
                var stringContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BotAuthenticationResponse>(stringContent);
                _lastBearerToken = responseObject.Token;

                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _lastBearerToken);
                return true;
            }

            LogRequestError(TokenRoute, response.StatusCode);
            return false;
        }

        private void LogRequestError(string route,
                                     string reason)
        {
            _logger.LogWarning("Failed to call '{HttpAddress}{Route}': {Reason}", _appSettings.UnitsBaseAddress, route, reason);
        }

        private void LogRequestError(string route,
                                     HttpStatusCode statusCode)
        {
            _logger.LogWarning("Failed to call '{HttpAddress}{Route}': {Reason} {HttpStatusCodeName} {HttpStatusCode}", _appSettings.UnitsBaseAddress, route, "HTTP Status Code", statusCode.ToString(), (int)statusCode);
        }

        async Task<string[]> IUnitsAccess.GetValidRoleNamesAsync()
        {
            string[] methodResult = null;

            async Task<HttpResponseMessage> ExecuteHttpCall(HttpClient httpClient)
            {
                try
                {
                    return await httpClient.GetAsync(RoleNamesRoute);
                }
                catch (HttpRequestException e)
                {
                    var baseExceptionMessage = e.GetBaseException().Message;
                    LogRequestError(RoleNamesRoute, baseExceptionMessage);
                    return null;
                }
            }

            async Task HandleResponseMessage(HttpResponseMessage responseMessage)
            {
                if (responseMessage == null)
                    return;
                if (!responseMessage.IsSuccessStatusCode)
                {
                    LogRequestError(RoleNamesRoute, responseMessage.StatusCode);
                    return;
                }

                var stringContent = await responseMessage.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<string[]>(stringContent);
                methodResult = result;
            }

            await UseHttpClient(ExecuteHttpCall, HandleResponseMessage);

            return methodResult;
        }

        async Task<SyncAllUsersResponse> IUnitsAccess.SendAllUsersAsync(UserModel[] users)
        {
            SyncAllUsersResponse methodResult = null;

            var request = new SyncAllUsersRequest
            {
                Users = users
            };
            var serialized = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(serialized, Encoding.UTF8, "application/json");

            async Task<HttpResponseMessage> ExecuteHttpCall(HttpClient httpClient)
            {
                try
                {
                    return await httpClient.PostAsync(PushAllUsersRoute, requestContent);
                }
                catch (HttpRequestException e)
                {
                    var baseExceptionMessage = e.GetBaseException().Message;
                    LogRequestError(PushAllUsersRoute, baseExceptionMessage);
                    return null;
                }
            }

            async Task HandleResponseMessage(HttpResponseMessage responseMessage)
            {
                if (responseMessage == null)
                    return;
                if (responseMessage.IsSuccessStatusCode)
                {
                    var responseContent = await responseMessage.Content.ReadAsStringAsync();
                    methodResult = JsonConvert.DeserializeObject<SyncAllUsersResponse>(responseContent);
                }
                else
                {
                    LogRequestError(PushAllUsersRoute, responseMessage.StatusCode);
                }
            }

            await UseHttpClient(ExecuteHttpCall, HandleResponseMessage);

            return methodResult;
        }
    }
}