using HoU.GuildBot.Shared.Objects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HoU.GuildBot.WebHost;

public static class RootSettingsLoader
{
    internal static void AddRootSettings(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var appSettings = LoadRootSettingsFromConfiguration(configuration);
        serviceCollection.AddSingleton(appSettings);
    }

    private static RootSettings LoadRootSettingsFromConfiguration(IConfiguration configuration)
    {
        var seqSection = configuration.GetRequiredSection("Seq");
        var discordSection = configuration.GetRequiredSection("Discord");

        return new RootSettings(configuration.GetConnectionString("HandOfUnityGuild"),
                                configuration.GetConnectionString("HangFire"),
                                seqSection.GetValue<string>("serverUrl"),
                                seqSection.GetValue<string>("apiKey"),
                                discordSection.GetValue<string>("botToken"),
                                configuration);
    }
}