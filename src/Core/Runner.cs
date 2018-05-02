namespace HoU.GuildBot.Core
{
    using System;
    using System.IO;
    using AWS.Logger.AspNetCore;
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
        private static readonly Version BotVersion = new Version(0, 4, 3);

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
                BotToken = settingsSection[nameof(AppSettings.BotToken)],
                HandOfUnityGuildId = ulong.Parse(settingsSection[nameof(AppSettings.HandOfUnityGuildId)]),
                LoggingChannelId = ulong.Parse(settingsSection[nameof(AppSettings.LoggingChannelId)]),
                PromotionAnnouncementChannelId = ulong.Parse(settingsSection[nameof(AppSettings.PromotionAnnouncementChannelId)])
            };
            if (string.IsNullOrWhiteSpace(settings.BotToken))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.BotToken)}' cannot be empty.");
            if (settings.HandOfUnityGuildId == 0)
                throw new InvalidOperationException($"AppSettings '{nameof(AppSettings.HandOfUnityGuildId)}' must be a correct ID.");
            if (settings.LoggingChannelId == 0)
                throw new InvalidOperationException($"AppSettings '{nameof(AppSettings.LoggingChannelId)}' must be a correct ID.");
            if (settings.PromotionAnnouncementChannelId == 0)
                throw new InvalidOperationException($"AppSettings '{nameof(AppSettings.PromotionAnnouncementChannelId)}' must be a correct ID.");

            // Prepare IoC
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton(settings);
            RegisterLogging(serviceCollection, configuration, environment);
            RegisterDAL(serviceCollection);
            RegisterBLL(serviceCollection, environment);
            var serviceProvider = serviceCollection.BuildServiceProvider();

            _logger = serviceProvider.GetService<ILogger<Runner>>();

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
                .AddSingleton<IGuildUserRegistry, GuildUserRegistry>()
                .AddSingleton<IGuildUserPromoter, GuildUserPromoter>()
                .AddSingleton<IBotInformationProvider>(botInformationProvider)
                .AddSingleton<IHelpProvider, HelpProvider>();
        }

        private static void RegisterDAL(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDiscordAccess, DiscordAccess>();
        }

        private static void RegisterLogging(IServiceCollection serviceCollection, IConfiguration configuration, string environment)
        {
            serviceCollection.AddLogging(builder =>
            {
                builder.AddConfiguration(configuration.GetSection("Logging"));
                switch (environment)
                {
                    case "Development":
                        builder.AddDebug();
                        break;
                    case "Production":
                        builder.AddProvider(GetAwsProvider(configuration));
                        break;
                }
            });
        }

        private static ILoggerProvider GetAwsProvider(IConfiguration configuration)
        {
            return new AWSLoggerProvider(configuration.GetAWSLoggingConfigSection(),
                (level, message, exception) => $"[{level}]: {exception ?? message}");
        }
    }
}