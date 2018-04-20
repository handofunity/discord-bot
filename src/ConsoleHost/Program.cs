namespace HoU.GuildBot.ConsoleHost
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
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
            
            // Resolve bot engine
            var botEngine = serviceProvider.GetService<IBotEngine>();

            // Actually run
            botEngine.Run(arguments).GetAwaiter().GetResult();
        }
    }
}
