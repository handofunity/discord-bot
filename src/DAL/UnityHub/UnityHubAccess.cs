using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HoU.GuildBot.DAL.UnityHub
{
    public class UnityHubAccess : IUnityHubAccess
    {
        private const string PushAllUsersRoute = "/discordEndPoint.php";
        
        private readonly ILogger<UnityHubAccess> _logger;

        public UnityHubAccess(ILogger<UnityHubAccess> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

//        private async Task UseHttpClient(Func<HttpClient, Task<HttpResponseMessage>> invokeHttpRequest,
//                                         Func<HttpResponseMessage, Task> handleResult)
//        {
//            using (var httpClientHandler = new HttpClientHandler())
//            {
//#if DEBUG
//                httpClientHandler.ServerCertificateCustomValidationCallback = (message,
//                                                                               certificate2,
//                                                                               arg3,
//                                                                               arg4) => true;
//#endif
//                using (var httpClient = new HttpClient(httpClientHandler) { BaseAddress = new Uri(_appSettings.UnityHubBaseAddress) })
//                {
//                    httpClient.DefaultRequestHeaders.Accept.Clear();
//                    httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

//                    var response = await invokeHttpRequest(httpClient);
//                    await handleResult(response);
//                }
//            }
//        }

//        private void LogRequestError(string route,
//                                     string reason)
//        {
//            _logger.LogWarning("Failed to call '{HttpAddress}{Route}': {Reason}", _appSettings.UnityHubBaseAddress, route, reason);
//        }

//        private void LogRequestError(string route,
//                                     HttpStatusCode statusCode)
//        {
//            _logger.LogWarning("Failed to call '{HttpAddress}{Route}': {Reason} {HttpStatusCodeName} {HttpStatusCode}", _appSettings.UnityHubBaseAddress, route, "HTTP Status Code", statusCode.ToString(), (int)statusCode);
//        }

        string[] IUnityHubAccess.GetValidRoleNames()
        {
            throw new NotSupportedException("Currently not supported.");
            //return new[]
            //{
            //    "Leader",
            //    "Officer",
            //    "Lead Community Coordinator",
            //    "Community Coordinator",
            //    "AoC Coordinator",
            //    "WoW Classic Coordinator",
            //    "Member",
            //    "Trial Member"
            //};
        }

        async Task<bool> IUnityHubAccess.SendAllUsersAsync(UserModel[] users)
        {
            throw new NotSupportedException("Currently not supported.");
            //var methodResult = false;

            //var request = new SyncAllUsersRequest
            //{
            //    Users = users
            //};
            //var serialized = JsonConvert.SerializeObject(request);
            //var requestContent = new MultipartFormDataContent
            //{
            //    {new StringContent(_appSettings.UnityHubAccessSecret, Encoding.UTF8, "plain/text"), "key"},
            //    {new StringContent(serialized, Encoding.UTF8, "application/json"), "users"}
            //};

            //async Task<HttpResponseMessage> ExecuteHttpCall(HttpClient httpClient)
            //{
            //    try
            //    {
            //        return await httpClient.PostAsync(PushAllUsersRoute, requestContent);
            //    }
            //    catch (HttpRequestException e)
            //    {
            //        var baseExceptionMessage = e.GetBaseException().Message;
            //        LogRequestError(PushAllUsersRoute, baseExceptionMessage);
            //        return null;
            //    }
            //}

            //async Task HandleResponseMessage(HttpResponseMessage responseMessage)
            //{
            //    if (responseMessage == null)
            //        return;
                
            //    if (responseMessage.IsSuccessStatusCode)
            //    {
            //        methodResult = true;
            //    }
            //    else
            //    {
            //        // Try to read the response body when we receive a no-success status code.
            //        try
            //        {
            //            var content = await responseMessage.Content.ReadAsStringAsync();
            //            if (string.IsNullOrWhiteSpace(content))
            //            {
            //                LogRequestError(PushAllUsersRoute, responseMessage.StatusCode);
            //            }
            //            else
            //            {
            //                LogRequestError(PushAllUsersRoute, content);
            //            }
            //        }
            //        catch
            //        {
            //            // If we can't read the response body, just use the status code.
            //            LogRequestError(PushAllUsersRoute, responseMessage.StatusCode);
            //        }
            //    }
            //}

            //await UseHttpClient(ExecuteHttpCall, HandleResponseMessage);

            //return methodResult;
        }
    }
}