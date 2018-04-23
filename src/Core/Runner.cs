namespace HoU.GuildBot.Core
{
    using System;
    using System.IO;
    using BLL;
    using DAL;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    public class Runner
    {
        private static readonly Version BotVersion = new Version(0, 1, 6);

        private ILogger<Runner> _logger;

        public void Run(string environment)
        {
            // Retrieve settings
            var builder = new ConfigurationBuilder()
                          .SetBasePath(Directory.GetCurrentDirectory())
                          .AddJsonFile($"appsettings.{environment}.json");
            var configuration = builder.Build();
            var settingsSection = configuration.GetSection("AppSettings");
            var settings = new AppSettings
            {
                BotToken = settingsSection[nameof(AppSettings.BotToken)]
            };

            // Prepare IoC
            var serviceCollection = new ServiceCollection();
            RegisterLogging(serviceCollection, environment);
            RegisterDAL(serviceCollection);
            RegisterBLL(serviceCollection, environment);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _logger = serviceProvider.GetService<ILogger<Runner>>();

            // Resolve bot engine
            var botEngine = serviceProvider.GetService<IBotEngine>();

            // Actually run
            botEngine.Run(settings).GetAwaiter().GetResult();
        }

        public void NotifyShutdown(string message)
        {
            _logger.LogCritical("Unexpected shutdown: " + message);
        }

        private static void RegisterBLL(IServiceCollection serviceCollection, string environment)
        {
            var runtimeInformation = new RuntimeInformation(
                environment,
                DateTime.Now.ToUniversalTime(),
                BotVersion);
            var botInformationProvider = new BotInformationProvider(runtimeInformation);
            serviceCollection.AddSingleton<IBotInformationProvider>(botInformationProvider);
            serviceCollection.AddSingleton<IBotEngine, BotEngine>();
        }

        private static void RegisterDAL(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDiscordAccess, DiscordAccess>();
        }

        private static void RegisterLogging(IServiceCollection serviceCollection, string environment)
        {
            serviceCollection.AddLogging(configure =>
            {
                switch (environment)
                {
                    case "Development":
                        configure.AddDebug();
                        configure.AddConsole();
                        break;
                    case "Production":
                        // Currently no logging for production
                        break;
                }
            });
        }
    }
}