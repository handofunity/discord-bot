using System;
using System.Threading.Tasks;
using HoU.GuildBot.Core;
using HoU.GuildBot.Shared.Objects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace HoU.GuildBot.WebHost;

public static class Program
{
    private static readonly Runner _runner;

    static Program()
    {
        _runner = new Runner();
    }

    public static async Task Main()
    {
        try
        {
            Startup.EnvironmentConfigured += Startup_EnvironmentConfigured;
            var host = BuildWebHost();
            await host.RunAsync(ApplicationLifecycle.CancellationToken);
        }
        catch (Exception e)
        {
            _runner.NotifyShutdown(e.ToString());
        }

        _runner.NotifyShutdown("no reason specified");
        Environment.FailFast("Shutting down process due to lacking connection.");
    }

    private static IWebHost BuildWebHost() =>
        Microsoft.AspNetCore.WebHost.CreateDefaultBuilder()
                 .ConfigureAppConfiguration(configurationBuilder =>
                  {
                      configurationBuilder.AddJsonFile("appsettings.json", true, true)
                                          .AddUserSecrets(typeof(Program).Assembly, true, true)
                                          .AddEnvironmentVariables("SETTINGS_OVERRIDE_");
                  })
                 .UseStartup<Startup>()
                 .Build();

    private static void Startup_EnvironmentConfigured(object? sender,
                                                      EnvironmentEventArgs e)
    {
        Startup.EnvironmentConfigured -= Startup_EnvironmentConfigured;
        _runner.Run(e.Environment,
                    e.RootSettings,
                    ApplicationLifecycle.CancellationToken);
    }
}