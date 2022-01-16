using System;
using System.Collections;
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
            await PrintEnvironmentVariablesAsync();
            Startup.EnvironmentConfigured += Startup_EnvironmentConfigured;
            await Console.Out.WriteLineAsync("Building web host ...");
            var host = BuildWebHost();
            await Console.Out.WriteLineAsync("Running web host ...");
            await host.RunAsync(ApplicationLifecycle.CancellationToken);
        }
        catch (Exception e)
        {
            await Console.Error.WriteLineAsync(e.ToString());
            _runner.NotifyShutdown(e.ToString());
        }

        _runner.NotifyShutdown("no reason specified");
        Environment.FailFast("Shutting down process due to lacking connection.");
    }

    private static async Task PrintEnvironmentVariablesAsync()
    {
#if DEBUG
        await Console.Out.WriteLineAsync("Environment variables are not exposed during debugging.");
#else
        await Console.Out.WriteLineAsync("Environment variables:");
        foreach (DictionaryEntry env in Environment.GetEnvironmentVariables())
        {
            await Console.Out.WriteLineAsync($"{env.Key}={env.Value}");
        }
#endif
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