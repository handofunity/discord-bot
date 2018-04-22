namespace HoU.GuildBot.WebHost
{
    using System;

    public class EnvironmentEventArgs : EventArgs
    {
        public string Environment { get; }

        public EnvironmentEventArgs(string environment)
        {
            Environment = environment;
        }
    }
}