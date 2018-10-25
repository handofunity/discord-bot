namespace HoU.GuildBot.Core
{
    using System;
    using System.IO;
    using BLL;
    using DAL.Database;
    using DAL.Discord;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using NLog.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;
    using DiscordUserEventHandler = BLL.DiscordUserEventHandler;

    public class Runner
    {
        private static readonly Version BotVersion = new Version(1, 7, 0);

        private ILogger<Runner> _logger;

        public void Run(string environment, AppSettings settings)
        {
            // Retrieve settings

            // Prepare IoC
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(settings);
            RegisterLogging(serviceCollection, settings.LoggingConfiguration, environment);
            RegisterDAL(serviceCollection);
            RegisterBLL(serviceCollection, environment);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _logger = serviceProvider.GetService<ILogger<Runner>>();
            _logger.LogInformation($"Starting up {nameof(IBotEngine)}...");

            // Resolve bot engine
            var botEngine = serviceProvider.GetService<IBotEngine>();

            // Actually run
            botEngine.Run().GetAwaiter().GetResult();
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

            serviceCollection
                .AddSingleton<IBotEngine, BotEngine>()
                .AddSingleton<ISpamGuard, SpamGuard>()
                .AddSingleton<IIgnoreGuard, IgnoreGuard>()
                .AddSingleton<ICommandRegistry, CommandRegistry>()
                .AddSingleton<IDiscordUserEventHandler, DiscordUserEventHandler>()
                .AddSingleton<IBotInformationProvider>(botInformationProvider)
                .AddSingleton<IHelpProvider, HelpProvider>()
                .AddSingleton<IMessageProvider, MessageProvider>()
                .AddSingleton<IVacationProvider, VacationProvider>()
                .AddSingleton<IPrivacyProvider, PrivacyProvider>()
                .AddSingleton<IGameRoleProvider, GameRoleProvider>()
                .AddSingleton<IGuildInfoProvider, GuildInfoProvider>()
                .AddSingleton<IUserInfoProvider, UserInfoProvider>()
                .AddSingleton<IUserStore, UserStore>()
                .AddSingleton<IStaticMessageProvider, StaticMessageProvider>()
                .AddSingleton<IStatisticImageProvider, StatisticImageProvider>();
        }

        private static void RegisterDAL(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDatabaseAccess, DatabaseAccess>();
            serviceCollection.AddSingleton<IDiscordAccess, DiscordAccess>();
        }

        private static void RegisterLogging(IServiceCollection serviceCollection, IConfiguration loggingConfiguration, string environment)
        {
            serviceCollection.AddLogging(builder =>
            {
                builder.AddConfiguration(loggingConfiguration);
                switch (environment)
                {
                    case "Development":
                        builder.AddDebug();
                        break;
                    case "Production":
                        builder.AddNLog();
                        break;
                }
            });
        }
    }
}