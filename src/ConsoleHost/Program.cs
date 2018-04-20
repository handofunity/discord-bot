namespace HoU.GuildBot.ConsoleHost
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using BLL;
    using DAL;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    internal static class Program
    {
        public static void Main()
        {
            // Prepare IoC
            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection
                .AddSingleton<IBotEngine, BotEngine>()
                .AddSingleton<IDiscordAccess, DiscordAccess>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Retrieve arguments
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            var configuration = builder.Build();
            var arguments = new BotEngineArguments
            {
                BotToken = configuration["discord:botToken"]
            };

            // Run
            var botEngine = serviceProvider.GetService<IBotEngine>();
            botEngine.Run(arguments).GetAwaiter().GetResult();
#if DEBUG
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
#endif
        }
    }
}
