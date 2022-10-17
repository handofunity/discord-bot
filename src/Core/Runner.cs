using System.Net.Http;
using System.Net.Http.Headers;
using HoU.GuildBot.DAL.Keycloak;
using DiscordUserEventHandler = HoU.GuildBot.BLL.DiscordUserEventHandler;

namespace HoU.GuildBot.Core;

public class Runner
{
    private static readonly Version _botVersion = new(10, 0, 1);

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
        catch (Exception e)
        {
            Log.Fatal(e, "Error running bot");
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Break();
#endif
            throw;
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
        _logger?.LogCritical("Unexpected shutdown: {Message}", message);
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
           .AddSingleton<IMenuRegistry, MenuRegistry>()
            // Transients
           .AddTransient<IVacationProvider, VacationProvider>()
           .AddTransient<IBirthdayProvider, BirthdayProvider>()
           .AddTransient<IGuildInfoProvider, GuildInfoProvider>()
           .AddTransient<IUserInfoProvider, UserInfoProvider>()
           .AddTransient<IImageProvider, ImageProvider>()
           .AddTransient<ITimeInformationProvider, TimeInformationProvider>()
           .AddTransient<IKeycloakSyncService, KeycloakSyncService>()
           .AddTransient<IScheduledReminderProvider, ScheduledReminderProvider>()
           .AddTransient<IKeycloakDiscordComparer, KeycloakDiscordComparer>()
            // Triggered as scheduled HangFire job
           .AddTransient<UnityHubSyncService>()
           .AddTransient<ReminderService>();
    }

    private static void RegisterDAL(IServiceCollection serviceCollection)
    {
        serviceCollection.AddHttpClient("units")
                         .ConfigureHttpClient(client =>
                          {
                              client.DefaultRequestHeaders.Accept.Clear();
                              client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                          })
                         .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                          {
#if DEBUG
                              ServerCertificateCustomValidationCallback = (_, _, _, _) => true
#endif
                          });
        serviceCollection.AddHttpClient("keycloak")
                         .ConfigureHttpClient(client =>
                          {
                              client.DefaultRequestHeaders.Accept.Clear();
                              client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                          });
        serviceCollection.AddSingleton<IConfigurationDatabaseAccess, ConfigurationDatabaseAccess>();
        serviceCollection.AddSingleton<IDatabaseAccess, DatabaseAccess>();
        serviceCollection.AddSingleton<IDiscordAccess, DiscordAccess>();
        serviceCollection.AddSingleton<IWebAccess, WebAccess>();
        serviceCollection.AddSingleton<IBearerTokenManager<UnitsAccess>, BearerTokenManager<UnitsAccess>>();
        serviceCollection.AddSingleton<IBearerTokenManager<KeycloakAccess>, BearerTokenManager<KeycloakAccess>>();
        serviceCollection.AddSingleton<IUnitsAccess, UnitsAccess>();
        serviceCollection.AddSingleton<IUnitsSignalRClient, UnitsSignalRClient>();
        serviceCollection.AddSingleton<IKeycloakAccess, KeycloakAccess>();
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

        Log.Information("Initialized logger");

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