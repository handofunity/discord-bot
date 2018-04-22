namespace HoU.GuildBot.WebHost
{
    using System;
    using System.Diagnostics;
    using JetBrains.Annotations;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class Startup
    {
        public static event EventHandler<EnvironmentEventArgs> EnvironmentConfigured;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Trace.WriteLine($"Executing '{nameof(ConfigureServices)}' ...");
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            Trace.WriteLine($"Executing '{nameof(Configure)}' ...");
            string environment;
            if (env.IsDevelopment())
            {
                environment = "Development";
            }
            else if (env.IsProduction())
            {
                environment = "Production";
            }
            else
            {
                throw new InvalidOperationException("Environment is not supported.");
            }
            EnvironmentConfigured?.Invoke(this, new EnvironmentEventArgs(environment));
        }
    }
}