using System;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.WebHost
{
    public class EnvironmentEventArgs : EventArgs
    {
        public string Environment { get; }

        public AppSettings AppSettings { get; }

        public EnvironmentEventArgs(string environment,
                                    AppSettings appSettings)
        {
            Environment = environment;
            AppSettings = appSettings;
        }
    }
}