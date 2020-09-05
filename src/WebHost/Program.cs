using System;
using System.Threading.Tasks;
using HoU.GuildBot.Core;
using Microsoft.AspNetCore.Hosting;

namespace HoU.GuildBot.WebHost
{
    public static class Program
    {
        private static readonly Runner _runner;

        static Program()
        {
            _runner = new Runner();
        }

        public static void Main()
        {
            try
            {
                Task.Run(() => Startup.EnvironmentConfigured += Startup_EnvironmentConfigured);
                var host = BuildWebHost();
                host.Run();
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
                     .UseStartup<Startup>()
                     .Build();

        private static void Startup_EnvironmentConfigured(object sender,
                                                          EnvironmentEventArgs e)
        {
            _runner.Run(e.Environment, e.AppSettings);
        }
    }
}