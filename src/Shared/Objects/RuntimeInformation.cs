namespace HoU.GuildBot.Shared.Objects
{
    using System;

    public class RuntimeInformation
    {
        public string Environment { get; }

        public DateTime StartTime { get; }

        public RuntimeInformation(string environment,
                                  DateTime startTime)
        {
            Environment = environment;
            StartTime = startTime;
        }
    }
}