namespace HoU.GuildBot.WebHost
{
    using System;
    using System.Linq;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Shared.Objects;

    internal static class AppSettingsLoader
    {
        internal static void AddAppSettings(this IServiceCollection serviceCollection, IConfiguration configuration)
        {
            var appSettings = LoadAppSettingsFromConfiguration(configuration);
            serviceCollection.AddSingleton(appSettings);
        }

        private static AppSettings LoadAppSettingsFromConfiguration(IConfiguration configuration)
        {
            // Deserialize
            var settings = configuration.GetSection("AppSettings").Get<AppSettings>(options => options.BindNonPublicProperties = true);

            // Read settings not in the AppSettings section
            settings.HandOfUnityConnectionString = configuration.GetConnectionString("HandOfUnityGuild");
            settings.LoggingConfiguration = configuration.GetSection("Logging");

            // Validate everything to make sure that not an empty or incomplete setting file is used
            ValidateSettings(settings);

            return settings;
        }

        private static void ValidateSettings(AppSettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.BotToken))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.BotToken)}' cannot be empty.");
            if (settings.HandOfUnityGuildId == 0)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.HandOfUnityGuildId)}' must be a correct ID.");
            if (settings.LoggingChannelId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.LoggingChannelId)}' must be a correct ID.");
            if (settings.InfoAndRolesChannelId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.InfoAndRolesChannelId)}' must be a correct ID.");
            if (settings.PromotionAnnouncementChannelId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.PromotionAnnouncementChannelId)}' must be a correct ID.");
            if (settings.AshesOfCreationRoleChannelId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.AshesOfCreationRoleChannelId)}' must be a correct ID.");
            if (settings.WorldOfWarcraftRoleChannelId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.WorldOfWarcraftRoleChannelId)}' must be a correct ID.");
            if (settings.GamesRolesChannelId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.GamesRolesChannelId)}' must be a correct ID.");
            if (settings.FriendOrGuestMessageId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.FriendOrGuestMessageId)}' must be a correct ID.");
            if (settings.NonMemberGameInterestMessageId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.NonMemberGameInterestMessageId)}' must be a correct ID.");
            if (settings.VoiceChannelCategoryId == default)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.VoiceChannelCategoryId)}' must be a correct ID.");
            if (settings.DesiredTimeZones.Length == 0)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.DesiredTimeZones)}' cannot be empty.");
            if (settings.SpamLimits.Length == 0)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.SpamLimits)}' cannot be empty.");
            if (settings.SpamLimits.Count(m => m.RestrictToChannelID == null) != 1)
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.SpamLimits)}' must have exactly one entry without the {nameof(SpamLimit.RestrictToChannelID)} set.");
            if (settings.SpamLimits.GroupBy(m => m.RestrictToChannelID).Any(m => m.Count() > 1))
                throw new InvalidOperationException($"AppSetting '{nameof(AppSettings.SpamLimits)}' contains duplicate {nameof(SpamLimit.RestrictToChannelID)}s.");
        }
    }
}