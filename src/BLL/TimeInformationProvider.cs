using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.Shared.BLL;

namespace HoU.GuildBot.BLL;

public class TimeInformationProvider : ITimeInformationProvider
{
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly ILogger<TimeInformationProvider> _logger;
        
    public TimeInformationProvider(IDynamicConfiguration dynamicConfiguration,
                                   ILogger<TimeInformationProvider> logger)
    {
        _dynamicConfiguration = dynamicConfiguration;
        _logger = logger;
    }
        
    string[] ITimeInformationProvider.GetCurrentTimeFormattedForConfiguredTimezones()
    {
        var n = DateTime.UtcNow;
        var supportedTimeZones = TimeZoneInfo.GetSystemTimeZones();
        var result = new List<string>(_dynamicConfiguration.DesiredTimeZones.Length);

        _logger.LogDebug($"Desired time zones: {_dynamicConfiguration.DesiredTimeZones.Length}");
        _logger.LogDebug($"Supported time zones: {supportedTimeZones.Count}");

        foreach (var desiredTimeZone in _dynamicConfiguration.DesiredTimeZones)
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
}