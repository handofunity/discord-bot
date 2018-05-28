namespace HoU.GuildBot.WebHost
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Core;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;

    public static class Program
    {
        private static readonly Runner Runner;

        static Program()
        {
            Runner = new Runner();
        }

        public static void Main()
        {
            try
            {
                Task.Run(() => Startup.EnvironmentConfigured += Startup_EnvironmentConfigured);
                var host = BuildWebHost();
                host.WaitForShutdown();
            }
            catch (Exception e)
            {
                Runner.NotifyShutdown(e.ToString());
            }
            Runner.NotifyShutdown("no reason specified");
        }

        private static IWebHost BuildWebHost() =>
            WebHost.CreateDefaultBuilder()
                   .UseKestrel()
                   .UseContentRoot(Directory.GetCurrentDirectory())
                   .UseIISIntegration()
                   .UseStartup<Startup>()
                   .Build();

        private static void Startup_EnvironmentConfigured(object sender, EnvironmentEventArgs e)
        {
            Runner.Run(e.Environment);
        }
    }
}
