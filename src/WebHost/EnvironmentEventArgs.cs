using System;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.WebHost
{
    public class EnvironmentEventArgs : EventArgs
    {
        public string Environment { get; }

        public RootSettings RootSettings { get; }

        public EnvironmentEventArgs(string environment,
                                    RootSettings rootSettings)
        {
            Environment = environment;
            RootSettings = rootSettings;
        }
    }
}