using System;
using System.Diagnostics;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HoU.GuildBot.Shared.Objects;
using Microsoft.Extensions.Hosting;

namespace HoU.GuildBot.WebHost
{
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

            services.AddAppSettings(Configuration);
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

            var settings = app.ApplicationServices.GetService<AppSettings>();
            EnvironmentConfigured?.Invoke(this, new EnvironmentEventArgs(environment, settings));
        }
    }
}