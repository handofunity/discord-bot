namespace HoU.GuildBot.Core
{
    using System;
    using BLL;
    using DAL;
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
        private static readonly Version BotVersion = new Version(2, 7, 0);

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
                // Singletons
               .AddSingleton<IBotEngine, BotEngine>()
               .AddSingleton<IUserStore, UserStore>()
               .AddSingleton<ISpamGuard, SpamGuard>()
               .AddSingleton<IIgnoreGuard, IgnoreGuard>()
               .AddSingleton<ICommandRegistry, CommandRegistry>()
               .AddSingleton<IDiscordUserEventHandler, DiscordUserEventHandler>()
               .AddSingleton<IBotInformationProvider>(botInformationProvider)
               .AddSingleton<IMessageProvider, MessageProvider>()
               .AddSingleton<IPrivacyProvider, PrivacyProvider>()
               .AddSingleton<IGameRoleProvider, GameRoleProvider>()
               .AddSingleton<IStaticMessageProvider, StaticMessageProvider>()
               // Transients
               .AddTransient<IHelpProvider, HelpProvider>()
               .AddTransient<IVacationProvider, VacationProvider>()
               .AddTransient<IGuildInfoProvider, GuildInfoProvider>()
               .AddTransient<IUserInfoProvider, UserInfoProvider>()
               .AddTransient<IImageProvider, ImageProvider>()
               .AddTransient<ITimeInformationProvider, TimeInformationProvider>();
        }

        private static void RegisterDAL(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDatabaseAccess, DatabaseAccess>();
            serviceCollection.AddSingleton<IDiscordAccess, DiscordAccess>();
            serviceCollection.AddSingleton<IWebAccess, WebAccess>();
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