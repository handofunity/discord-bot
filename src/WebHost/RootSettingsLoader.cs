using System;
using System.Text;
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
        var ownConnectionString = ParseConnectionString(configuration.GetConnectionString("HandOfUnityGuild"));
        var hangFireConnectionString = ParseConnectionString(configuration.GetConnectionString("HangFire"));

        return new RootSettings(ownConnectionString,
                                hangFireConnectionString,
                                seqSection.GetValue<string>("serverUrl"),
                                seqSection.GetValue<string>("apiKey"),
                                discordSection.GetValue<string>("botToken"),
                                configuration);
    }

    private static string ParseConnectionString(string source)
    {
        const string base64Prefix = "base64:";

        if (!source.StartsWith(base64Prefix))
            return source;

        var encodedSource = source.Replace(base64Prefix, string.Empty);
        // Ensure that the padding is correct.
        encodedSource = encodedSource.PadRight(encodedSource.Length + (4 - encodedSource.Length % 4) % 4, '=');

        var decodedBytes = Convert.FromBase64String(encodedSource);
        var result = Encoding.UTF8.GetString(decodedBytes);

        Console.Out.WriteLine($"Decoded connection string into: {result}");

        return result;
    }
}