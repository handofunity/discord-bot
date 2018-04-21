namespace HoU.GuildBot.ConsoleHost
{
    using System.IO;
    using Core;

    internal static class Program
    {
        public static void Main()
        {
            Runner.Run(Directory.GetCurrentDirectory());
        }
    }
}
