namespace HoU.GuildBot.ConsoleHost
{
    using Core;

    internal static class Program
    {
        public static void Main()
        {
            var runner = new Runner();
            runner.Run("Development");
        }
    }
}
