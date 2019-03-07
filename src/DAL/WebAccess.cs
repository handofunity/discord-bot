namespace HoU.GuildBot.DAL
{
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Shared.DAL;

    public class WebAccess : IWebAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<WebAccess> _logger;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public WebAccess(ILogger<WebAccess> logger)
        {
            _logger = logger;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IWebAccess Members

        async Task<byte[]> IWebAccess.GetDiscordAvatarByUrl(string url)
        {
            if (url == null)
                return null;

            try
            {
                using (var client = new HttpClient())
                {
                    using (var result = await client.GetAsync(url))
                    {
                        return result.IsSuccessStatusCode
                                   ? await result.Content.ReadAsByteArrayAsync()
                                   : null;
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to load Discord avatar for URL: {url}");
                return null;
            }
        }

        #endregion
    }
}