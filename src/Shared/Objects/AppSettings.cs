namespace HoU.GuildBot.Shared.Objects
{
    using Microsoft.Extensions.Configuration;
    using StrongTypes;

    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the bot token.
        /// </summary>
        public string BotToken { get; set; }

        /// <summary>
        /// Gets or sets the Discord ID of the "Hand of Unity" guild.
        /// </summary>
        public ulong HandOfUnityGuildId { get; set; }

        /// <summary>
        /// Gets the Discord ID of the channel used for logging.
        /// </summary>
        public DiscordChannelID LoggingChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="LoggingChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong LoggingChannelIdValue
        {
            get => (ulong) LoggingChannelId;
            set => LoggingChannelId = (DiscordChannelID) value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel used for infos and public basic roles.
        /// </summary>
        public DiscordChannelID InfoAndRolesChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="InfoAndRolesChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong InfoAndRolesChannelIdValue
        {
            get => (ulong)InfoAndRolesChannelId;
            set => InfoAndRolesChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel used for promotion announcements.
        /// </summary>
        public DiscordChannelID PromotionAnnouncementChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="PromotionAnnouncementChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong PromotionAnnouncementChannelIdValue
        {
            get => (ulong)PromotionAnnouncementChannelId;
            set => PromotionAnnouncementChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel that is used for the 'Ashes of Creation' role feature.
        /// </summary>
        public DiscordChannelID AshesOfCreationRoleChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="AshesOfCreationRoleChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong AshesOfCreationRoleChannelIdValue
        {
            get => (ulong)AshesOfCreationRoleChannelId;
            set => AshesOfCreationRoleChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel that is used for the 'World of Warcraft' role feature.
        /// </summary>
        public DiscordChannelID WorldOfWarcraftRoleChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="WorldOfWarcraftRoleChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong WorldOfWarcraftRoleChannelIdValue
        {
            get => (ulong)WorldOfWarcraftRoleChannelId;
            set => WorldOfWarcraftRoleChannelId = (DiscordChannelID)value;
        }

        /// <summary>
        /// Gets the Discord ID of the channel that is used for the 'Games' role feature.
        /// </summary>
        public DiscordChannelID GamesRolesChannelId { get; private set; }

        /// <summary>
        /// Property to bind the value of <see cref="GamesRolesChannelId"/> from the app settings.
        /// </summary>
        // ReSharper disable once UnusedMember.Local
        private ulong GamesRolesChannelIdValue
        {
            get => (ulong) GamesRolesChannelId;
            set => GamesRolesChannelId = (DiscordChannelID) value;
        }

        /// <summary>
        /// Gets or sets the message ID used for friend and guest reactions.
        /// </summary>
        public ulong FriendOrGuestMessageId { get; set; }

        /// <summary>
        /// Gets or sets the message ID used for non-member game interest reactions.
        /// </summary>
        public ulong NonMemberGameInterestMessageId { get; set; }

        /// <summary>
        /// Gets or sets an array of <see cref="DesiredTimeZone"/> instances.
        /// </summary>
        public DesiredTimeZone[] DesiredTimeZones { get; set; }

        /// <summary>
        /// Gets or sets an array of <see cref="SpamLimit"/> instances.
        /// </summary>
        public SpamLimit[] SpamLimits { get; set; }

        /// <summary>
        /// Gets or sets an array of channel IDs that will have the spam protection disabled.
        /// </summary>
        public ulong[] ChannelIDsWithDisabledSpamProtection { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <remarks>Should either be a IP/TCP connection, or a named connection. For IP/TCP connections, see example.</remarks>
        /// <example>IPv4: "Server=169.100.10.154\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
        /// IPv6: "Server=fe80::2011:f831:9281:1ffb%23\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
        /// IPv6: "Server=fe80::2011:f831:9281:1ffb\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"</example>
        public string HandOfUnityConnectionString { get; set; }

        public IConfiguration LoggingConfiguration { get; set; }
    }
}