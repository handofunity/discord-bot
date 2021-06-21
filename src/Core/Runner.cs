using HoU.GuildBot.DAL.UNITS;
using HoU.GuildBot.DAL.UnityHub;
using System;
using HoU.GuildBot.BLL;
using HoU.GuildBot.DAL;
using HoU.GuildBot.DAL.Database;
using HoU.GuildBot.DAL.Discord;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using Serilog.Exceptions;
using Serilog.Exceptions.Core;
using Serilog.Exceptions.Destructurers;
using Serilog.Exceptions.EntityFrameworkCore.Destructurers;
using Serilog.Exceptions.MsSqlServer.Destructurers;
using DiscordUserEventHandler = HoU.GuildBot.BLL.DiscordUserEventHandler;

namespace HoU.GuildBot.Core
{
    public class Runner
    {
        private BackgroundJobServer _backgroundJobServer;

        private static readonly Version _botVersion = new(5, 5, 0);

        private ILogger<Runner> _logger;

        public void Run(string environment, AppSettings settings)
        {
            try
            {
                // Prepare IoC
                var serviceCollection = new ServiceCollection();
                serviceCollection.AddSingleton(settings);
                RegisterLogging(serviceCollection, settings.CompleteConfiguration);
                RegisterDAL(serviceCollection);
                RegisterBLL(serviceCollection, environment);
                RegisterHangFire(serviceCollection, settings);
                var serviceProvider = serviceCollection.BuildServiceProvider();

                _logger = serviceProvider.GetService<ILogger<Runner>>();
                _logger.LogInformation($"Starting up {nameof(IBotEngine)}...");

                RunHangFireServer(serviceProvider);
                RunBotEngine(serviceProvider);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private void RunHangFireServer(IServiceProvider serviceProvider)
        {
            GlobalConfiguration.Configuration.UseActivator(new ContainerJobActivator(serviceProvider));

            var jobStorage = serviceProvider.GetService<JobStorage>();
            _backgroundJobServer = new BackgroundJobServer(jobStorage);
        }

        private static void RunBotEngine(IServiceProvider serviceProvider)
        {
            // Resolve bot engine
            var botEngine = serviceProvider.GetService<IBotEngine>();

            // Actually run
            botEngine?.Run().GetAwaiter().GetResult();
        }

        public void NotifyShutdown(string message)
        {
            _backgroundJobServer?.Dispose();
            _logger.LogCritical("Unexpected shutdown: " + message);
        }

        private static void RegisterBLL(IServiceCollection serviceCollection, string environment)
        {
            var runtimeInformation = new RuntimeInformation(
                environment,
                DateTime.Now.ToUniversalTime(),
                _botVersion);
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
               .AddSingleton<INonMemberRoleProvider, NonMemberRoleProvider>()
               .AddSingleton<IGameRoleProvider, GameRoleProvider>()
               .AddSingleton<IStaticMessageProvider, StaticMessageProvider>()
               .AddSingleton<IVoiceChannelManager, VoiceChannelManager>()
               .AddSingleton<IUnitsBotClient, UnitsBotClient>()
               // Transients
               .AddTransient<IHelpProvider, HelpProvider>()
               .AddTransient<IVacationProvider, VacationProvider>()
               .AddTransient<IGuildInfoProvider, GuildInfoProvider>()
               .AddTransient<IUserInfoProvider, UserInfoProvider>()
               .AddTransient<IImageProvider, ImageProvider>()
               .AddTransient<ITimeInformationProvider, TimeInformationProvider>()
               .AddTransient<IUnitsSyncService, UnitsSyncService>()
                // Triggered as scheduled HangFire job
               .AddTransient<UnityHubSyncService>()
               .AddTransient<PersonalReminderService>();
        }

        private static void RegisterDAL(IServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<IDatabaseAccess, DatabaseAccess>();
            serviceCollection.AddSingleton<IDiscordAccess, DiscordAccess>();
            serviceCollection.AddSingleton<IWebAccess, WebAccess>();
            serviceCollection.AddSingleton<IUnitsBearerTokenManager, UnitsBearerTokenManager>();
            serviceCollection.AddSingleton<IUnitsAccess, UnitsAccess>();
            serviceCollection.AddSingleton<IUnitsSignalRClient, UnitsSignalRClient>();
            serviceCollection.AddSingleton<IUnityHubAccess, UnityHubAccess>();
        }

        private static void RegisterLogging(IServiceCollection serviceCollection, IConfiguration completeConfiguration)
        {
            Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(completeConfiguration)
                        .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                                                    .WithDefaultDestructurers()
                                                    .WithDestructurers(new IExceptionDestructurer[]
                                                     {
                                                         new SqlExceptionDestructurer(),
                                                         new DbUpdateExceptionDestructurer()
                                                     }))
                        .CreateLogger();

            Log.Information("Initialized logger.");

            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog();
            });
        }

        private void RegisterHangFire(ServiceCollection serviceCollection,
                                      AppSettings settings)
        {
            serviceCollection.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                      .UseSimpleAssemblyNameTypeSerializer()
                      .UseRecommendedSerializerSettings()
                      .UseSqlServerStorage(settings.HangFireConnectionString,
                                           new SqlServerStorageOptions
                                           {
                                               CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                                               SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                                               QueuePollInterval = TimeSpan.Zero,
                                               UseRecommendedIsolationLevel = true,
                                               UsePageLocksOnDequeue = true,
                                               DisableGlobalLocks = true
                                           });
            });
            serviceCollection.AddHangfireServer();
        }
    }
}