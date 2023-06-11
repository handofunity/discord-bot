namespace HoU.GuildBot.WebHost;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public class Startup
{
    public static event EventHandler<EnvironmentEventArgs>? EnvironmentConfigured;

    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        Trace.WriteLine($"Executing '{nameof(ConfigureServices)}' ...");

        services.AddRootSettings(Configuration);
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        Trace.WriteLine($"Executing '{nameof(Configure)}' ...");
        string environment;
        if (env.IsDevelopment())
        {
            environment = Constants.RuntimeEnvironment.Development;
        }
        else if (env.IsProduction())
        {
            environment = Constants.RuntimeEnvironment.Production;
        }
        else
        {
            throw new InvalidOperationException("Environment is not supported.");
        }

        var settings = app.ApplicationServices.GetRequiredService<RootSettings>();
        EnvironmentConfigured?.Invoke(this, new EnvironmentEventArgs(environment, settings));
    }
}