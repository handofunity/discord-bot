using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.Objects;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace HoU.GuildBot.DAL.UNITS
{
    public class UnitsBearerTokenManager : IUnitsBearerTokenManager
    {
        private readonly ILogger<UnitsBearerTokenManager> _logger;
        private const string TokenRoute = "/bot-api/auth/token";

        private readonly Dictionary<string, string> _lastBearerTokens;

        public UnitsBearerTokenManager(ILogger<UnitsBearerTokenManager> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _lastBearerTokens = new Dictionary<string, string>();
        }

        public async Task<bool> GetAndSetBearerToken(HttpClient httpClient,
                                                      string baseAddress,
                                                      string secret,
                                                      bool refresh)
        {
            var bearerToken = await GetBearerTokenAsync(httpClient, baseAddress, secret, refresh);
            if (bearerToken == null)
                return false;

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            return true;
        }

        public async Task<string> GetBearerTokenAsync(HttpClient httpClient,
                                                      string baseAddress,
                                                      string secret,
                                                      bool forceRefresh)
        {
            if (!forceRefresh && _lastBearerTokens.TryGetValue(baseAddress, out var lastToken))
            {
                // Check if token expires within the next 10 seconds.
                var token = new JwtSecurityToken(lastToken);
                if (token.ValidTo > DateTime.UtcNow.AddSeconds(10))
                    return lastToken;
            }

            HttpResponseMessage response;
            try
            {
                var request = new BotAuthenticationRequest
                {
                    ClientId = typeof(UnitsAccess).FullName,
                    ClientSecret = secret
                };
                var serialized = JsonConvert.SerializeObject(request);
                response = await httpClient.PostAsync(TokenRoute, new StringContent(serialized, Encoding.UTF8, "application/json"));
            }
            catch (HttpRequestException e)
            {
                var baseExceptionMessage = e.GetBaseException().Message;
                _logger.LogRequestError(baseAddress, TokenRoute, baseExceptionMessage);
                return null;
            }

            if (response.IsSuccessStatusCode)
            {
                var stringContent = await response.Content.ReadAsStringAsync();
                var responseObject = JsonConvert.DeserializeObject<BotAuthenticationResponse>(stringContent);
                _lastBearerTokens[baseAddress] = responseObject.Token;
                return responseObject.Token;
            }

            _logger.LogRequestError(baseAddress, TokenRoute, response.StatusCode);
            return null;
        }
    }
}