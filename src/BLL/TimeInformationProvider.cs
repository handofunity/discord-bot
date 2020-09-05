using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
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
            var result = new List<string>(_settings.DesiredTimeZones.Length);

            _logger.LogDebug($"Desired time zones: {_settings.DesiredTimeZones.Length}");
            _logger.LogDebug($"Supported time zones: {supportedTimeZones.Count}");

            foreach (var desiredTimeZone in _settings.DesiredTimeZones)
            {
                if (desiredTimeZone.TimeZoneId == "UTC")
                {
                    result.Add($"# {n:ddd} {n:T} (UTC       - {desiredTimeZone.InvariantDisplayName})");
                }
                else
                {
                    var tz = supportedTimeZones.SingleOrDefault(m => m.Id == desiredTimeZone.TimeZoneId);
                    if (tz == null)
                    {
                        _logger.LogWarning($"Desired time zone '{desiredTimeZone}' is not available.");
                        continue;
                    }

                    var localTime = TimeZoneInfo.ConvertTimeFromUtc(n, tz);
                    result.Add($"> {n:ddd} {localTime:T} ({(tz.IsDaylightSavingTime(localTime) ? tz.DaylightName : tz.StandardName),-9} - {desiredTimeZone.InvariantDisplayName})");
                }
            }

            return result.ToArray();
        }

        #endregion
    }
}