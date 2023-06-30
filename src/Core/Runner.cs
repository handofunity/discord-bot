namespace HoU.GuildBot.Core;

public class Runner
{
    private static readonly Version _botVersion = new(10, 6, 0);

    private BackgroundJobServer? _backgroundJobServer;
    private ILogger<Runner>? _logger;

    public void Run(string environment,
                    RootSettings settings,
                    CancellationToken cancellationToken)
    {
        try
        {
            // Prepare IoC
            IServiceCollection services = new ServiceCollection();

            services.AddSingleton(settings)
                    .AddLogging(settings.CompleteConfiguration)
                    .AddDataAccessLayer()
                    .AddBusinessLogicLayer(environment, _botVersion)
                    .AddHangfireWithServer(settings)
                    .AddKeycloak();
            
            var serviceProvider = services.BuildServiceProvider();

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

        var jobStorage = serviceProvider.GetRequiredService<JobStorage>();
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
}