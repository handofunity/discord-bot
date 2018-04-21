namespace HoU.GuildBot.Core
{
#if DEBUG
    using System.Diagnostics;
#endif
    using BLL;
    using DAL;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    public static class Runner
    {
        public static void Run(string configurationBasePath)
        {
            // Prepare IoC
            var serviceProvider = new ServiceCollection()
                                  .RegisterBLL()
                                  .RegisterDAL()
                                  .RegisterLogging()
                                  .BuildServiceProvider();

            // Retrieve arguments
            var builder = new ConfigurationBuilder()
                          .SetBasePath(configurationBasePath)
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

        private static IServiceCollection RegisterBLL(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IBotEngine, BotEngine>();
            return serviceCollection;
        }

        private static IServiceCollection RegisterDAL(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDiscordAccess, DiscordAccess>();
            return serviceCollection;
        }

        private static IServiceCollection RegisterLogging(this IServiceCollection serviceCollection)
        {
            serviceCollection.AddLogging(configure =>
            {
#if DEBUG
                if (Debugger.IsAttached)
                    configure.AddDebug();
                configure.AddConsole();
#endif
            });
            return serviceCollection;
        }
    }
}