namespace HoU.GuildBot.DAL
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Newtonsoft.Json;
    using Shared.DAL;
    using Shared.Objects;

    public class UnitsAccess : IUnitsAccess
    {
        private const string TokenEndpoint = "/bot-api/auth/token";
        private const string RoleNamesEndpoint = "/bot-api/discordsync/valid-role-names";
        private const string PushAllUsersEndpoint = "/bot-api/discordsync/all-users";

        private readonly IDiscordAccess _discordAccess;
        private readonly AppSettings _appSettings;

        private string _lastBearerToken;

        public UnitsAccess(IDiscordAccess discordAccess,
                           AppSettings appSettings)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
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
                response = await httpClient.PostAsync(TokenEndpoint, new StringContent(serialized, Encoding.UTF8, "application/json"));
            }
            catch (HttpRequestException e)
            {
                var baseExceptionMessage = e.GetBaseException().Message;
                await LogRequestErrorAsync(TokenEndpoint, baseExceptionMessage);
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

            await LogRequestErrorAsync(TokenEndpoint, $"HTTP {(int) response.StatusCode} {response.StatusCode}");
            return false;
        }

        private async Task LogRequestErrorAsync(string endpoint,
                                                string message)
        {
            await _discordAccess.LogToDiscord($"{nameof(UnitsAccess)} - Failed to call '{_appSettings.UnitsBaseAddress}{endpoint}': {message}");
        }

        async Task<string[]> IUnitsAccess.GetValidRoleNamesAsync()
        {
            string[] methodResult = null;

            async Task<HttpResponseMessage> ExecuteHttpCall(HttpClient httpClient)
            {
                try
                {
                    return await httpClient.GetAsync(RoleNamesEndpoint);
                }
                catch (HttpRequestException e)
                {
                    var baseExceptionMessage = e.GetBaseException().Message;
                    await LogRequestErrorAsync(RoleNamesEndpoint, baseExceptionMessage);
                    return null;
                }
            }

            async Task HandleResponseMessage(HttpResponseMessage responseMessage)
            {
                if (responseMessage == null)
                    return;
                if (!responseMessage.IsSuccessStatusCode)
                {
                    await LogRequestErrorAsync(RoleNamesEndpoint, $"HTTP {(int)responseMessage.StatusCode} {responseMessage.StatusCode}");
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
                    return await httpClient.PostAsync(PushAllUsersEndpoint, requestContent);
                }
                catch (HttpRequestException e)
                {
                    var baseExceptionMessage = e.GetBaseException().Message;
                    await LogRequestErrorAsync(PushAllUsersEndpoint, baseExceptionMessage);
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
                    await LogRequestErrorAsync(PushAllUsersEndpoint, $"HTTP {(int) responseMessage.StatusCode} {responseMessage.StatusCode}");
                }
            }

            await UseHttpClient(ExecuteHttpCall, HandleResponseMessage);

            return methodResult;
        }
    }
}