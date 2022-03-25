using HoU.GuildBot.DAL.UNITS;
using HoU.GuildBot.DAL.UnityHub;
using System;
using System.Threading;
using HoU.GuildBot.BLL;
using HoU.GuildBot.DAL;
using HoU.GuildBot.DAL.Database;
using HoU.GuildBot.DAL.Discord;
using Hangfire;
using Hangfire.PostgreSql;
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
using DiscordUserEventHandler = HoU.GuildBot.BLL.DiscordUserEventHandler;

namespace HoU.GuildBot.Core;

public class Runner
{
    private static readonly Version _botVersion = new(9, 0, 0);

    private BackgroundJobServer? _backgroundJobServer;
    private ILogger<Runner>? _logger;
        
    public void Run(string environment,
                    RootSettings settings,
                    CancellationToken cancellationToken)
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

            _logger = serviceProvider.GetRequiredService<ILogger<Runner>>();
            _logger.LogInformation($"Starting up {nameof(IBotEngine)}...");

            _logger.LogTrace("Loading dynamic configuration ...");
            var dynamicConfiguration = serviceProvider.GetRequiredService<IDynamicConfiguration>();
            dynamicConfiguration.LoadAllDataAsync().GetAwaiter().GetResult();

            RunHangFireServer(serviceProvider);
            RunBotEngine(serviceProvider, cancellationToken);
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private void RunHangFireServer(IServiceProvider serviceProvider)
    {
        _logger?.LogTrace("Starting HangFire server ...");
        GlobalConfiguration.Configuration.UseActivator(new ContainerJobActivator(serviceProvider));

        var jobStorage = serviceProvider.GetService<JobStorage>();
        _backgroundJobServer = new BackgroundJobServer(jobStorage);
    }

    private void RunBotEngine(IServiceProvider serviceProvider,
                              CancellationToken cancellationToken)
    {
        _logger?.LogTrace("Starting bot engine ...");

        // Resolve bot engine
        var botEngine = serviceProvider.GetRequiredService<IBotEngine>();

        // Actually run
        botEngine.Run(cancellationToken).GetAwaiter().GetResult();
    }

    public void NotifyShutdown(string message)
    {
        _backgroundJobServer?.Dispose();
        _logger?.LogCritical("Unexpected shutdown: " + message);
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
           .AddSingleton<IDiscordUserEventHandler, DiscordUserEventHandler>()
           .AddSingleton<IBotInformationProvider>(botInformationProvider)
           .AddSingleton<IMessageProvider, MessageProvider>()
           .AddSingleton<IPrivacyProvider, PrivacyProvider>()
           .AddSingleton<INonMemberRoleProvider, NonMemberRoleProvider>()
           .AddSingleton<IGameRoleProvider, GameRoleProvider>()
           .AddSingleton<IStaticMessageProvider, StaticMessageProvider>()
           .AddSingleton<IUnitsBotClient, UnitsBotClient>()
           .AddSingleton<IRoleRemover, RoleRemover>()
           .AddSingleton<IDynamicConfiguration, DynamicConfiguration>()
            // Transients
           .AddTransient<IVacationProvider, VacationProvider>()
           .AddTransient<IGuildInfoProvider, GuildInfoProvider>()
           .AddTransient<IUserInfoProvider, UserInfoProvider>()
           .AddTransient<IImageProvider, ImageProvider>()
           .AddTransient<ITimeInformationProvider, TimeInformationProvider>()
           .AddTransient<IUnitsSyncService, UnitsSyncService>()
            // Triggered as scheduled HangFire job
           .AddTransient<UnityHubSyncService>()
           .AddTransient<ReminderService>();
    }

    private static void RegisterDAL(IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IConfigurationDatabaseAccess, ConfigurationDatabaseAccess>();
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
        var seqSection = completeConfiguration.GetRequiredSection("Seq");
        var seqServerUrl = seqSection.GetValue<string>("serverUrl");
        var seqApiKey = seqSection.GetValue<string>("apiKey");
        var loggerConfiguration = new LoggerConfiguration()
                                 .ReadFrom.Configuration(completeConfiguration)
                                 .WriteTo.Seq(seqServerUrl, apiKey: seqApiKey)
                                 .Enrich.WithExceptionDetails(new DestructuringOptionsBuilder()
                                                             .WithDefaultDestructurers()
                                                             .WithDestructurers(new IExceptionDestructurer[]
                                                              {
                                                                  new DbUpdateExceptionDestructurer()
                                                              }));

#if DEBUG
        loggerConfiguration.WriteTo.Debug();
#endif

        Log.Logger = loggerConfiguration.CreateLogger();

        Log.Information("Initialized logger.");

        serviceCollection.AddLogging(builder =>
        {
            builder.AddSerilog();
        });
    }

    private void RegisterHangFire(ServiceCollection serviceCollection,
                                  RootSettings settings)
    {
        serviceCollection.AddHangfire(config =>
        {
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UsePostgreSqlStorage(settings.ConnectionStringForHangFireDatabase,
                                        new PostgreSqlStorageOptions
                                        {
                                            SchemaName = "hang_fire",
                                            PrepareSchemaIfNecessary = true,
                                            QueuePollInterval = TimeSpan.FromSeconds(15),
                                            InvisibilityTimeout = TimeSpan.FromMinutes(5),
                                            DistributedLockTimeout = TimeSpan.FromMinutes(2),
                                            UseNativeDatabaseTransactions = true
                                        });
        });
        serviceCollection.AddHangfireServer();
    }
}