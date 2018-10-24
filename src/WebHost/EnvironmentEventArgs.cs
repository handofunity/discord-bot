namespace HoU.GuildBot.WebHost
{
    using System;
    using Shared.Objects;

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