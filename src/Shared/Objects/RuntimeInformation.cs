using System;

namespace HoU.GuildBot.Shared.Objects
{
    public class RuntimeInformation
    {
        public string Environment { get; }

        public DateTime StartTime { get; }

        public Version Version { get; }

        public RuntimeInformation(string environment,
                                  DateTime startTime,
                                  Version version)
        {
            Environment = environment;
            StartTime = startTime;
            Version = version;
        }
    }
}