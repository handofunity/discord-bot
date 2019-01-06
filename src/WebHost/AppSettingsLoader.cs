namespace HoU.GuildBot.WebHost
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.Objects;
    using Shared.StrongTypes;

    internal static class AppSettingsLoader
    {
        internal static void AddAppSettings(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var appSettings = new AppSettings
            {
                HandOfUnityConnectionString = configuration.GetConnectionString("HandOfUnityGuild"),
                LoggingConfiguration = configuration.GetSection("Logging")
            };
            appSettings.LoadAdditionalAppSettingsFromConfiguration(configuration);

            serviceCollection.AddSingleton(appSettings);
        }

        private static void LoadAdditionalAppSettingsFromConfiguration(this AppSettings appSettings, IConfiguration configuration)
        {
            ReadAdditionalSettings(appSettings, configuration);
            ValidateSettings(appSettings);
        }

        private static void ReadAdditionalSettings(AppSettings appSettings, IConfiguration configuration)
        {
            var settingsSection = configuration.GetSection("AppSettings");

            appSettings.BotToken = settingsSection[nameof(AppSettings.BotToken)];
            appSettings.HandOfUnityGuildId = ulong.Parse(settingsSection[nameof(AppSettings.HandOfUnityGuildId)]);
            appSettings.LoggingChannelId = (DiscordChannelID) ulong.Parse(settingsSection[nameof(AppSettings.LoggingChannelId)]);
            appSettings.PromotionAnnouncementChannelId = (DiscordChannelID) ulong.Parse(settingsSection[nameof(AppSettings.PromotionAnnouncementChannelId)]);
            appSettings.WelcomeChannelId = (DiscordChannelID) ulong.Parse(settingsSection[nameof(AppSettings.WelcomeChannelId)]);
            appSettings.AshesOfCreationRoleChannelId = (DiscordChannelID) ulong.Parse(settingsSection[nameof(AppSettings.AshesOfCreationRoleChannelId)]);
            appSettings.DesiredTimeZoneIDs = settingsSection.GetSection(nameof(AppSettings.DesiredTimeZoneIDs)).GetChildren().Select(m => m.Value).ToArray();
        }

        private static void ValidateSettings(AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.BotToken))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.BotToken)}' cannot be empty.");
            if (settings.HandOfUnityGuildId == 0)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.HandOfUnityGuildId)}' must be a correct ID.");
            if (settings.LoggingChannelId == default(DiscordChannelID))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.LoggingChannelId)}' must be a correct ID.");
            if (settings.PromotionAnnouncementChannelId == default(DiscordChannelID))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.PromotionAnnouncementChannelId)}' must be a correct ID.");
            if (settings.WelcomeChannelId == default(DiscordChannelID))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.WelcomeChannelId)}' must be a correct ID.");
            if (settings.AshesOfCreationRoleChannelId == default(DiscordChannelID))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.AshesOfCreationRoleChannelId)}' must be a correct ID.");
            if (settings.DesiredTimeZoneIDs.Length == 0)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.DesiredTimeZoneIDs)}' cannot be empty.");
        }
    }
}