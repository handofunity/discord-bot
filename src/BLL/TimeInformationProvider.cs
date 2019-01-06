namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.Objects;

    public class TimeInformationProvider : ITimeInformationProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly AppSettings _settings;
        private readonly ILogger<TimeInformationProvider> _logger;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public TimeInformationProvider(AppSettings settings,
                                       ILogger<TimeInformationProvider> logger)
        {
            _settings = settings;
            _logger = logger;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region ITimeInformationProvider Members

        string[] ITimeInformationProvider.GetCurrentTimeFormattedForConfiguredTimezones()
        {
            var n = DateTime.UtcNow;
            var supportedTimeZones = TimeZoneInfo.GetSystemTimeZones();
            var result = new List<string>(_settings.DesiredTimeZoneIDs.Length);

            _logger.LogDebug($"Desired time zones: {_settings.DesiredTimeZoneIDs.Length}");
            _logger.LogDebug($"Supported time zones: {supportedTimeZones.Count}");

            foreach (var desiredTimeZoneID in _settings.DesiredTimeZoneIDs)
            {
                if (desiredTimeZoneID == "UTC")
                {
                    result.Add($"# {n:ddd} {n:T} (UTC       - Guild Time)");
                }
                else
                {
                    var tz = supportedTimeZones.SingleOrDefault(m => m.Id == desiredTimeZoneID);
                    if (tz == null)
                    {
                        _logger.LogWarning($"Desired time zone '{desiredTimeZoneID}' is not available.");
                        continue;
                    }

                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(n, tz);
                    result.Add($"> {n:ddd} {localTime:T} ({(tz.IsDaylightSavingTime(localTime) ? tz.DaylightName : tz.StandardName),-9} - {tz.Id})");
                }
            }

            return result.ToArray();
        }

        #endregion
    }
}