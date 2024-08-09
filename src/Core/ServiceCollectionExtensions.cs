namespace HoU.GuildBot.Core;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection AddLogging(this IServiceCollection services,
                                                  IConfiguration completeConfiguration)
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

        services.AddLogging(builder => { builder.AddSerilog(); });

        return services;
    }

    internal static IServiceCollection AddDataAccessLayer(this IServiceCollection services)
    {
        services.AddHttpClient("units")
                .ConfigureHttpClient(client =>
                 {
                     client.DefaultRequestHeaders.Accept.Clear();
                     client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                 })
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
                 {
#if DEBUG
                     ServerCertificateCustomValidationCallback = (_,
                                                                  _,
                                                                  _,
                                                                  _) => true
#endif
                 });

        services.AddSingleton<IConfigurationDatabaseAccess, ConfigurationDatabaseAccess>()
                .AddSingleton<IDatabaseAccess, DatabaseAccess>()
                .AddSingleton<IDiscordAccess, DiscordAccess>()
                .AddSingleton<IDiscordLogger>(provider => provider.GetRequiredService<IDiscordAccess>())
                .AddSingleton<IWebAccess, WebAccess>()
                .AddSingleton<IUnitsAccess, UnitsAccess>()
                .AddSingleton<IUnitsSignalRClient, UnitsSignalRClient>()
                .AddTransient<IDiscordSyncClient, DiscordSyncClient>(provider =>
                 {
                     var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                     var httpClient = httpClientFactory.CreateClient("units");
                     return new DiscordSyncClient(httpClient);
                 })
                .AddTransient<IDiscordUserClient, DiscordUserClient>(provider =>
                {
                    var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                    var httpClient = httpClientFactory.CreateClient("units");
                    return new DiscordUserClient(httpClient);
                });

        return services;
    }

    internal static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services,
                                                             string environment,
                                                             Version botVersion)
    {
        var runtimeInformation = new RuntimeInformation(
                                                        environment,
                                                        DateTime.Now.ToUniversalTime(),
                                                        botVersion);
        var botInformationProvider = new BotInformationProvider(runtimeInformation);

        services
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
           .AddTransient<IScheduledReminderProvider, ScheduledReminderProvider>()
            // Triggered as scheduled HangFire job
           .AddTransient<ReminderService>();

        return services;
    }

    internal static IServiceCollection AddHangfireWithServer(this IServiceCollection services,
                                                             RootSettings settings)
    {
        services.AddHangfire(config =>
        {
            var options = new PostgreSqlStorageOptions
            {
                SchemaName = "hang_fire",
                PrepareSchemaIfNecessary = true,
                QueuePollInterval = TimeSpan.FromSeconds(15),
                InvisibilityTimeout = TimeSpan.FromMinutes(5),
                DistributedLockTimeout = TimeSpan.FromMinutes(2),
                UseNativeDatabaseTransactions = true
            };
            config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                  .UseSimpleAssemblyNameTypeSerializer()
                  .UseRecommendedSerializerSettings()
                  .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(settings.ConnectionStringForHangFireDatabase), options);
        });
        services.AddHangfireServer();
        return services;
    }
}