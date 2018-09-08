namespace HoU.GuildBot.Core
{
    using System;
    using Microsoft.Extensions.Configuration;
    using Shared.Objects;
    using Shared.StrongTypes;

    internal static class AppSettingsLoader
    {
        internal static AppSettings LoadAppSettings(this IConfiguration configuration)
        {
            var settings = ReadSettings(configuration);
            return ValidateSettings(settings);
        }

        private static AppSettings ReadSettings(IConfiguration configuration)
        {
            var settingsSection = configuration.GetSection("AppSettings");
            var settings = new AppSettings
            {
                BotToken = settingsSection[nameof(AppSettings.BotToken)],
                HandOfUnityGuildId = ulong.Parse(settingsSection[nameof(AppSettings.HandOfUnityGuildId)]),
                LoggingChannelId = (DiscordChannelID)ulong.Parse(settingsSection[nameof(AppSettings.LoggingChannelId)]),
                PromotionAnnouncementChannelId = (DiscordChannelID)ulong.Parse(settingsSection[nameof(AppSettings.PromotionAnnouncementChannelId)]),
                WelcomeChannelId = (DiscordChannelID)ulong.Parse(settingsSection[nameof(AppSettings.WelcomeChannelId)]),
                AshesOfCreationRoleChannelId = (DiscordChannelID)ulong.Parse(settingsSection[nameof(AppSettings.AshesOfCreationRoleChannelId)]),
                ConnectionString = settingsSection[nameof(AppSettings.ConnectionString)]
            };
            return settings;
        }

        private static AppSettings ValidateSettings(AppSettings settings)
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
            if (string.IsNullOrWhiteSpace(settings.ConnectionString))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.ConnectionString)}' cannot be empty.");

            return settings;
        }
    }
}