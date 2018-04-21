namespace HoU.GuildBot.WebHost
{
    using System.IO;
    using Core;

    public static class Program
    {
        public static void Main()
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var settingsDirectory = Path.Combine(currentDirectory, "bin");
#if DEBUG
            settingsDirectory = Path.Combine(settingsDirectory, "Debug");
#else
            settingsDirectory = Path.Combine(settingsDirectory, "Release");
#endif
            settingsDirectory = Path.Combine(settingsDirectory, "netcoreapp2.0");
            Runner.Run(settingsDirectory);
        }
    }
}
